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
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true, Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping};
        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

        return $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). Tu tarea es generar el contenido de una evaluación matemática rellenando un esqueleto JSON predefinido.   

REGLAS ESTRICTAS:
1. Recibirás un JSON con la estructura de la evaluación. El tema principal a evaluar es '{esqueleto.TemaPrincipal}'.
2. EJEMPLO DE REFERENCIA: Usa el siguiente problema como guía de estilo, nivel de dificultad y contexto para generar los nuevos problemas:    
   ""{ejemploTexto}""
3. Para cada elemento en la lista de 'Preguntas', redacta un problema matemático práctico y contextualizado (apropiado para educación primaria) en el campo 'Enunciado'. BASATE EN EL EJEMPLO DE REFERENCIA.
4. Usa los datos del problema que TU generaste para hacer los calculos de todos los incisos. NO USES LOS DATOS DEL EJEMPLO DE REFERENCIA PARA LOS CALCULOS, SOLO COMO GUÍA DE ESTILO Y CONTEXTO.
5. Para cada 'Inciso' dentro de la pregunta, rellena todos los campos.
   - 'ExpresionMatematica': OBLIGATORIO: DEBE ser una CADENA DE TEXTO (string) entre comillas dobles (ejemplo: """"1/2""""). NUNCA la escribas sin comillas.
   - Si 'EsCorrecta' es true: Resuelve el problema correctamente. Rellena 'expresionMatematica' con la operación mmatemática correcta. 
   - Si 'EsCorrecta' es false: SOLO USA la expresión de 'modeloMatematico' para generar el resultado. Sustituye los valores del problema que generaste en 'modeloMatematico'. 'ExpresionMatematica' = 'modeloMatematico' con los valores sustituidos.
 - 'ValorCalculado': Escribe el resultado final de la 'ExpresionMatematica'. OBLIGATORIO: DEBE ser una CADENA DE TEXTO (string) entre comillas dobles (ejemplo: """"1/2"""").
6. El formato de salida debe ser ÚNICA Y EXCLUSIVAMENTE el JSON modificado y relleno. No agregues etiquetas markdown (como ```json).


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

//- Suma de fracciones con distinto denominador: """"(a+c)/(b+d)""""

//- Multiplicación de fracciones """"(a*d)/(b*c)""""

//- División de fracciones  """"(a*b)/(c*d)""""