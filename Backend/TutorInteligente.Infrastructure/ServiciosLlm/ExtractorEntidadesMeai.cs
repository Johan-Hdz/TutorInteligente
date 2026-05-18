using System.Text.Json;
using Microsoft.Extensions.AI; // <-- Importante
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

// 1. EL CAMBIO CLAVE: Pedir IChatClient en el constructor, no ChatClient
public class ExtractorEntidadesMeai(IChatClient chatClient) : IExtractorEntidades
{
    public async Task<ParametrosEvaluacion> ExtraerAsync(string consultaDocente)
    {
        // PROMPT ACTUALIZADO: Basado en extracción de entidades puras (GraphRAG E.1)
        var promptSistema = @"Eres un asistente experto en educación matemática. Tu única tarea es extraer las entidades y parámetros de la solicitud de un docente para generar una evaluación.

Reglas de extracción:
1. TemaPrincipal: Extrae el tema matemático o concepto central exactamente como lo expresa o insinúa el docente (ej. 'adición de quebrados', 'sumas', 'geometría básica', 'denomindores diferentes o iguales'). No intentes adivinar el nombre oficial, solo extrae la intención.
2. CantidadPreguntas: El número de problemas solicitados. Si no se especifica, devuelve 3.
3. EsValida: True si la consulta está relacionada con matemáticas o educación. False si es una charla casual, insulto o tema fuera de dominio.
4. MensajeError: Si EsValida es false, explica brevemente por qué. Si es true, déjalo en null o vacío.

Responde ESTRICTAMENTE con un objeto JSON válido con esta estructura, sin bloques de código markdown:
{
  ""TemaPrincipal"": ""string"",
  ""EsValida"": true_o_false,
  ""CantidadPreguntas"": numero_entero,
  ""MensajeError"": ""string""
}";

        // Usar los objetos de MEAI
        var mensajes = new List<ChatMessage>
        {
            new ChatMessage(ChatRole.System, promptSistema),
            new ChatMessage(ChatRole.User, consultaDocente)
        };

        var opciones = new ChatOptions
        {
            ResponseFormat = ChatResponseFormat.Json,
            Temperature = 0.0f
        };

        try
        {
            // Usar el método unificado de MEAI
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);
            string jsonRespuesta = respuesta.Text ?? "{}";

            var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            var parametros = JsonSerializer.Deserialize<ParametrosEvaluacion>(jsonRespuesta, opcionesJson);

            return parametros ?? new ParametrosEvaluacion("", 0, false, "Error al deserializar la respuesta del LLM.");
        }
        catch (Exception ex)
        {
            // Manejo de errores de conexión o de la API de MEAI
            return new ParametrosEvaluacion("", 0, false, $"Error al comunicarse con el modelo de IA: {ex.Message}");
        }
    }
}


//var promptSistema = @"Eres un asistente experto en educación matemática. Tu única tarea es extraer las entidades y parámetros de la solicitud de un docente para generar una evaluación.

//Reglas de extracción:
//1. TemaPrincipal: Extrae el tema matemático o concepto central exactamente como lo expresa o insinúa el docente (ej. 'adición de quebrados', 'sumas', 'geometría básica', 'denomindores diferentes o iguales'). No intentes adivinar el nombre oficial, solo extrae la intención.
//2. GradoEscolar: Si se menciona, extráelo como número entero. Si no, devuelve 0.
//3. ConceptosClave: Lista de subtemas o palabras clave adicionales mencionadas.
//4. CantidadPreguntas: El número de problemas solicitados. Si no se especifica, devuelve 3.
//5. EsValida: True si la consulta está relacionada con matemáticas o educación. False si es una charla casual, insulto o tema fuera de dominio.
//6. MensajeError: Si EsValida es false, explica brevemente por qué. Si es true, déjalo en null o vacío.

//Responde ESTRICTAMENTE con un objeto JSON válido con esta estructura, sin bloques de código markdown:
//{
//  ""TemaPrincipal"": ""string"",
//  ""EsValida"": true_o_false,
//  ""CantidadPreguntas"": numero_entero,
//  ""MensajeError"": ""string""
//}";