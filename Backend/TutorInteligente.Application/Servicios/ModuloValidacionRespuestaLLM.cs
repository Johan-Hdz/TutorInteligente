using Microsoft.Extensions.Logging;
using System.Text.Json;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;
using NCalc;
using NCalc.Exceptions;

namespace TutorInteligente.Application.Servicios;

public class ModuloValidacionRespuestaLLM(ILogger<ModuloValidacionRespuestaLLM> logger) : IModuloValidacionRespuestaLLM
{
    public async Task<(bool EsValido, List<string> Errores, Evaluacion? EvaluacionValidada)> ValidarRespuestaLlm(string jsonLlmOutput)
    {
        var errores = new List<string>(); // <- NUEVO: Acumulador de errores

        try
        {
            jsonLlmOutput = jsonLlmOutput.Replace("```json", "").Replace("```", "").Trim();
            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true, ReadCommentHandling = JsonCommentHandling.Skip, AllowTrailingCommas = true };
            var evaluacion = JsonSerializer.Deserialize<Evaluacion>(jsonLlmOutput, opciones);

            if (evaluacion?.Preguntas == null)
            {
                errores.Add("El JSON devuelto no pudo ser parseado al modelo Evaluacion.");
                return (false, errores, null);
            }

            foreach (var pregunta in evaluacion.Preguntas)
            {
                bool tieneCorrecta = false;
                foreach (var inciso in pregunta.Incisos)
                {
                    if (inciso.EsCorrecta) tieneCorrecta = true;

                    double valorExpresion = EvaluarExpresion(inciso.ExpresionMatematica);
                    double valorCalculadoNum;

                    try
                    {
                        valorCalculadoNum = EvaluarExpresion(inciso.ValorCalculado);
                    }
                    catch (Exception)
                    {
                        errores.Add($"En la opción {inciso.Letra}: El valor '{inciso.ValorCalculado}' no es válido.");
                        continue; // <- IMPORTANTE: Continuar evaluando los demás en vez de salir
                    }

                    if (Math.Abs(valorExpresion - valorCalculadoNum) > 0.01)
                    {
                        string tipo = inciso.EsCorrecta ? "Correcta" : $"Incorrecta (Distractor: {inciso.TextoTema})";
                        errores.Add($"Error en opción {inciso.Letra} ({tipo}): La expresión '{inciso.ExpresionMatematica}' da {valorExpresion}, pero el LLM reportó '{inciso.ValorCalculado}'.");
                    }
                }

                if (!tieneCorrecta)
                    errores.Add($"La pregunta '{pregunta.Enunciado}' no tiene respuesta correcta.");
            }

            return (errores.Count == 0, errores, errores.Count == 0 ? evaluacion : null);
        }
        //catch (JsonException ex)
        //{
        //    logger.LogError(ex, "Error de estructura JSON proveniente del LLM");
        //    //return (false, $"Estructura JSON inválida: {ex.Message}", null);
        //}
        //catch (NCalcEvaluationException ex)
        //{
        //    // NCalc lanza EvaluationException si la fórmula tiene sintaxis inválida
        //    logger.LogError(ex, "El LLM devolvió una expresión matemática mal formada.");
        //    //return (false, $"Error de sintaxis matemática en la respuesta del LLM: {ex.Message}", null);
        //}
        catch (Exception ex)
        {
            errores.Add($"Error crítico procesando la respuesta: {ex.Message}");
            return (false, errores, null);
        }
    }

    private double EvaluarExpresion(string expresion)
    {
        if (string.IsNullOrWhiteSpace(expresion))
            throw new ArgumentException("La expresión matemática está vacía.");

        // TRUCO VITAL: NCalc, al igual que C#, trata "1/2" como división entera y da 0.
        // Para asegurar que evalúe como punto flotante (0.5), convertimos los enteros a decimales
        // Ejemplo: "1/2 + 1/6" se convierte en "1.0/2.0 + 1.0/6.0"
        string expresionFlotante = System.Text.RegularExpressions.Regex.Replace(expresion, @"\b(\d+)\b", "$1.0");

        // Instanciar y evaluar con NCalc
        var ncalcExpression = new Expression(expresionFlotante);

        if (ncalcExpression.HasErrors())
        {
            throw new NCalcEvaluationException($"NCalc detectó errores de sintaxis en: {expresion}");
        }

        var resultado = ncalcExpression.Evaluate();

        return Convert.ToDouble(resultado);
    }
}
#region Anterior
//public class ModuloValidacionRespuestaLLM : IModuloValidacionRespuestaLLM
//{
//    public (bool EsValido, string MensajeError, Evaluacion? EvaluacionValidada) ValidarRespuestaLlm(string jsonLlmOutput)
//    {
//        try
//        {
//            // Limpiar posibles residuos de markdown si el LLM ignoró la regla
//            jsonLlmOutput = jsonLlmOutput.Replace("```json", "").Replace("```", "").Trim();

//            var opciones = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
//            var evaluacion = JsonSerializer.Deserialize<Evaluacion>(jsonLlmOutput, opciones);

//            if (evaluacion?.Preguntas == null)
//                return (false, "El JSON devuelto no pudo ser parseado al modelo Evaluacion.", null);

//            foreach (var pregunta in evaluacion.Preguntas)
//            {
//                bool tieneCorrecta = false;
//                foreach (var inciso in pregunta.Incisos)
//                {
//                    if (inciso.EsCorrecta) tieneCorrecta = true;

//                    // Validar matemáticamente la expresión contra el valor calculado
//                    double valorReal = EvaluarExpresion(inciso.ExpresionMatematica);

//                    // Tolerancia por decimales/fracciones (ej: 1/3 = 0.3333)
//                    if (Math.Abs(valorReal - inciso.ValorCalculado) > 0.01)
//                    {
//                        string tipo = inciso.EsCorrecta ? "Correcta" : $"Incorrecta (Distractor: {inciso.TextoTema})";
//                        return (false, $"Error matemático en la opción {inciso.Letra} ({tipo}): La expresión '{inciso.ExpresionMatematica}' da {valorReal}, pero el LLM escribió {inciso.ValorCalculado}.", null);
//                    }
//                }

//                if (!tieneCorrecta)
//                    return (false, $"La pregunta '{pregunta.Enunciado}' no tiene marcada ninguna respuesta como correcta.", null);
//            }

//            return (true, "Validación exitosa.", evaluacion);
//        }
//        catch (JsonException ex)
//        {
//            logger.LogError(ex, "Error de estructura JSON proveniente del LLM");
//            return (false, $"Estructura JSON inválida: {ex.Message}", null);
//        }
//        catch (Exception ex)
//        {
//            logger.LogError(ex, "Error indeterminado durante la validación");
//            return (false, $"Error de validación: {ex.Message}", null);
//        }
//    }
//}
#endregion