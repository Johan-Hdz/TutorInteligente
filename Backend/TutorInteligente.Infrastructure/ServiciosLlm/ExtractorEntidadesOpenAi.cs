using System.Text.Json;
using Microsoft.Extensions.AI; // <-- Importante
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

// 1. EL CAMBIO CLAVE: Pedir IChatClient en el constructor, no ChatClient
public class ExtractorEntidadesMeai(IChatClient chatClient) : IExtractorEntidades
{
    public async Task<ParametrosEvaluacion> ExtraerAsync(string consultaDocente)
    {
        var promptSistema = @"Eres un asistente experto en educación matemática. 
Tu única tarea es extraer las entidades de la consulta del docente para evaluaciones de 1ro a 4to de primaria.
Responde estrictamente con un objeto JSON válido con esta estructura:
{
  ""TemaPrincipal"": ""string"",
  ""GradoEscolar"": numero_entero,
  ""ConceptosClave"": [""arreglo_de_strings""],
  ""EsValida"": true_o_false,
  ""MensajeError"": ""mensaje de error si no es un tema matemático""
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
            Temperature = 0.1f
        };

        try
        {
            // Usar el método unificado de MEAI
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);

            string jsonRespuesta = respuesta.Text ?? "{}";

            var opcionesJson = new JsonSerializerOptions { PropertyNameCaseInsensitive = true };
            return JsonSerializer.Deserialize<ParametrosEvaluacion>(jsonRespuesta, opcionesJson)!;
        }
        catch (Exception ex)
        {
            return new ParametrosEvaluacion("", 0, [], false, $"Error al procesar con IA: {ex.Message}");
        }
    }
}