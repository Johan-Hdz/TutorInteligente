using OpenAI.Chat;
using System.Text.Json;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class ExtractorEntidadesOpenAi(ChatClient chatClient) : IExtractorEntidades
{
    public async Task<ParametrosEvaluacion> ExtraerAsync(string consultaDocente)
    {
        // 1. Diseñamos el System Prompt forzando la salida en JSON
        var promptSistema = @"Eres un asistente experto en educación matemática. 
Tu única tarea es extraer las entidades de la consulta del docente y responder estrictamente con un objeto JSON válido con esta estructura exacta:
{
  ""TemaPrincipal"": ""string"",
  ""GradoEscolar"": numero_entero,
  ""ConceptosClave"": [""arreglo_de_strings""],
  ""EsValida"": true_o_false,
  ""MensajeError"": ""mensaje en caso de que la consulta no sea sobre matemáticas o no tenga sentido""
}";

        // 2. Preparamos los mensajes
        var mensajes = new List<ChatMessage>
        {
            new SystemChatMessage(promptSistema),
            new UserChatMessage(consultaDocente)
        };

        // 3. Configuramos las opciones para exigir JSON y bajar la creatividad (queremos determinismo)
        var opciones = new ChatCompletionOptions
        {
            ResponseFormat = ChatResponseFormat.CreateJsonObjectFormat(),
            Temperature = 0.1f
        };

        try
        {
            // 4. Llamamos a la API de OpenAI
            ChatCompletion completacion = await chatClient.CompleteChatAsync(mensajes, opciones);

            // 5. Extraemos el texto JSON de la respuesta
            string jsonRespuesta = completacion.Content[0].Text;

            // 6. Deserializamos al record de C# (ignorando mayúsculas/minúsculas por seguridad)
            var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ParametrosEvaluacion>(jsonRespuesta, opcionesJson)!;
        }
        catch (Exception ex)
        {
            // Si algo falla a nivel de red o API, devolvemos un estado no válido sin romper el sistema
            return new ParametrosEvaluacion("", 0, [], false, $"Error al procesar con IA: {ex.Message}");
        }
    }
}
