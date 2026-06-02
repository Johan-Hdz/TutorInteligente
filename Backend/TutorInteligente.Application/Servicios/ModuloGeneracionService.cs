using Microsoft.Extensions.AI;
using Microsoft.Extensions.Logging;
using System.Text.Json;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Servicios;

public class ModuloGeneracionService(
    ILLMClient llmClient,
    IModuloValidacionRespuestaLLM validador,
    ILogger<ModuloGeneracionService> logger) : IModuloGeneracionService
{
    public async Task<Evaluacion> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto)
    {
        int maxIntentos = 3;
        int intentoActual = 1;

        // 1. Inicializamos el historial de conversación
        var historialMensajes = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.User, ConstruirPromptInicial(esqueleto, ejemploTexto))
        };

        string ultimoJsonGenerado = string.Empty;

        // 2. Ciclo de Generación, Validación y Corrección
        while (intentoActual <= maxIntentos)
        {
            logger.LogInformation("Llamando al LLM. Intento {Intento} de {Max}", intentoActual, maxIntentos);

            // Le pasamos todo el historial al LLM
            ultimoJsonGenerado = await llmClient.EjecutarPromptAsync(historialMensajes, temperature: 0.0f);

            var validacion = await validador.ValidarRespuestaLlm(ultimoJsonGenerado);

            if (validacion.EsValido && validacion.EvaluacionValidada != null)
            {
                logger.LogInformation("Validación matemática exitosa en el intento {Intento}.", intentoActual);
                return validacion.EvaluacionValidada;
            }

            logger.LogWarning("Validación fallida en el intento {Intento}. Errores detectados: {Errores}", intentoActual, string.Join(" | ", validacion.Errores));

            if (intentoActual == maxIntentos)
            {
                throw new InvalidOperationException($"Fallaron los {maxIntentos} intentos. Últimos errores reportados: {string.Join(", ", validacion.Errores)}");
            }

            // 3. ACTUALIZAR EL CONTEXTO PARA EL SIGUIENTE INTENTO
            // A) Agregamos lo que el LLM nos contestó (para que sepa qué hizo mal)
            historialMensajes.Add(new ChatMessage(ChatRole.Assistant, ultimoJsonGenerado));

            // B) Agregamos nuestro regaño/instrucción de corrección
            historialMensajes.Add(new ChatMessage(ChatRole.User, ConstruirPromptCorreccion(validacion.Errores)));

            intentoActual++;
        }

        throw new InvalidOperationException("Flujo de generación interrumpido inesperadamente.");
    }

    private string ConstruirPromptInicial(Evaluacion esqueleto, string ejemploTexto)
    {
        // (Este método se queda EXACTAMENTE IGUAL a como lo tenías)
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

        return $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). Tu tarea es generar el contenido de una evaluación matemática rellenando un esqueleto JSON predefinido.   

REGLAS ESTRICTAS:
1. Recibirás un JSON con la estructura de la evaluación. El tema principal a evaluar es '{esqueleto.TemaPrincipal}'.
2. EJEMPLO DE REFERENCIA: Usa el siguiente problema como guía de estilo, nivel de dificultad y contexto para generar los nuevos problemas:    
   ""{ejemploTexto}""
3. Para cada elemento en la lista de 'Preguntas', redacta un problema matemático práctico y contextualizado (apropiado para educación primaria) en el campo 'Enunciado'. BASATE EN EL EJEMPLO DE REFERENCIA.
4. Para cada 'Inciso' dentro de la pregunta, rellena los campos asegurando que la 'ExpresionMatematica' SIEMPRE coincida matemáticamente con el 'ValorCalculado':
   - 'ExpresionMatematica': Escribe la operación aritmética pura. OBLIGATORIO: DEBE ser una CADENA DE TEXTO (string) entre comillas dobles (ejemplo: """"1/2 + 1/6""""). NUNCA la escribas sin comillas.
   - 'ValorCalculado': Escribe el resultado final de la expresión. OBLIGATORIO: DEBE ser una CADENA DE TEXTO (string) entre comillas dobles (ejemplo: """"1/2"""").
   - Si 'EsCorrecta' es true: Escribe la operación matemáticamente correcta que resuelve el problema y su resultado correcto.
   - Si 'EsCorrecta' es false: Escribe la operación INCORRECTA (el procedimiento erróneo que seguiría el alumno según el 'TextoTema') y su resultado incorrecto. Por ejemplo, si el distractor es sumar directo en 1/2 + 1/3, la ExpresionMatematica debe mostrar el error: """"(1+1)/(2+3)"""" y el ValorCalculado """"2/5"""". NUNCA escribas la fórmula correcta seguida de un resultado incorrecto.
5. El formato de salida debe ser ÚNICA Y EXCLUSIVAMENTE el JSON modificado y relleno. No agregues etiquetas markdown (como ```json).

ESQUELETO A RELLENAR:{jsonEsqueleto}";
    }

    private string ConstruirPromptCorreccion(List<string> errores)
    {
        // NOTA: Ya no le mandamos el JSON fallido porque ya está en el historial (ChatRole.Assistant).
        // Solo le mandamos los errores precisos.
        string listaErrores = string.Join("\n- ", errores);

        return $@"Tu respuesta anterior falló mi validación matemática automatizada. 

Aquí están los errores matemáticos y lógicos exactos que detecté en el JSON que acabas de generar:
- {listaErrores}

TU TAREA:
Corrige EXCLUSIVAMENTE los errores mencionados basándote en tu respuesta anterior. Asegúrate de que las operaciones matemáticas ('ExpresionMatematica') coincidan exactamente con el 'ValorCalculado'. Recuerda seguir TODAS las reglas originales (especialmente el uso de comillas para fracciones).
Devuelve ÚNICA Y EXCLUSIVAMENTE el JSON corregido y completo, sin etiquetas markdown ni explicaciones adicionales.";
    }
}


//public class ModuloGeneracionService(ILLMClient llmClient) : IModuloGeneracionService
//{
//    public async Task<string> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto)
//    {
//        // 1. Lógica pura de aplicación: Preparación de datos
//        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
//        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

//        // 2. Lógica de negocio/dominio: El Prompt pedagógico
//        string promptGraphRag = $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). Tu tarea es generar el contenido de una evaluación matemática rellenando un esqueleto JSON predefinido.   

//REGLAS ESTRICTAS:
//1. Recibirás un JSON con la estructura de la evaluación. El tema principal a evaluar es '{esqueleto.TemaPrincipal}'.
//2. EJEMPLO DE REFERENCIA: Usa el siguiente problema como guía de estilo, nivel de dificultad y contexto para generar los nuevos problemas:    
//   ""{ejemploTexto}""
//3. Para cada elemento en la lista de 'Preguntas', redacta un problema matemático práctico y contextualizado (apropiado para educación primaria) en el campo 'Enunciado'. BASATE EN EL EJEMPLO DE REFERENCIA.
//4. Para cada 'Inciso' dentro de la pregunta, rellena los campos matemáticos de la siguiente manera:
//   - 'ExpresionMatematica': Escribe la operación aritmética pura que resuelve o simula el inciso. OBLIGATORIO: Este valor DEBE ser una CADENA DE TEXTO (string) encerrada entre comillas dobles (ejemplo: """"1/2 + 1/6"""" o """"5 * 3""""). NUNCA escribas la fracción como un número crudo sin comillas.
//   - 'ValorCalculado': Escribe el resultado final de la expresión tal como debe mostrársele al alumno. OBLIGATORIO: DEBE ser una CADENA DE TEXTO (string) entre comillas dobles. Si es una fracción, mantenla como fracción (ejemplo: """"1/2""""). Jamás lo escribas como un número crudo.
//   - Si 'EsCorrecta' es true: La expresión debe resolver matemáticamente de forma correcta el problema planteado.
//   - Si 'EsCorrecta' es false: La expresión debe ser INCORRECTA, reflejando un error lógico o de procedimiento basado en el distractor: 'TextoTema'.
//5. El formato de salida debe ser ÚNICA Y EXCLUSIVAMENTE el JSON modificado y relleno. No agregues etiquetas markdown (como ```json).

//ESQUELETO A RELLENAR:{jsonEsqueleto}";

//        // 3. Delegar la ejecución a la capa de infraestructura
//        return await llmClient.EjecutarPromptAsync(promptGraphRag, temperature: 0.0f);
//    }
//}