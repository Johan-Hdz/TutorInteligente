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
        var promptSistema = @"Eres un asistente experto en educación matemática. Tu única tarea es extraer las entidades de la consulta del docente para generar evaluaciones matemáticas y la cantidad de preguntas que solicito el docente.
Debes mapear el tema principal solicitado ESTRICTAMENTE a uno de los siguientes temas oficiales de nuestro temario:
- Suma de fracciones con igual denominador
- Resta de fracciones con igual denominador
- Suma de fracciones con distinto denominador
- Resta de fracciones con distinto denominador
- Multiplicación de fracciones
- División de fracciones
- Mínimo común múltiplo
- Fracciones equivalentes
- Multiplicación de enteros
- División de enteros
- Suma de enteros
- Resta de enteros
- Simplificación de fracciones
- Recíproco de una fracción

Si el docente pide un tema que no se puede mapear semánticamente a ninguno de los temas de esta lista, debes marcar la consulta como inválida.

Responde estrictamente con un objeto JSON válido con esta estructura, sin bloques de código ni texto adicional:
{
  ""TemaPrincipal"": ""string (debe ser exactamente uno de los temas de la lista anterior, o null si no se encuentra)"",
  ""GradoEscolar"": numero_entero,
  ""ConceptosClave"": [""arreglo_de_strings""],
  ""EsValida"": true_o_false,
  ""CantidadPreguntas"": numero_entero (cantidad de preguntas que el docente solicita generar, o 0 si no se especifica)"",
  ""MensajeError"": ""mensaje de error si no es un tema matemático o si el tema solicitado no está en el temario oficial""
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
            return new ParametrosEvaluacion("",0, [], false, $"Error al procesar con IA: {ex.Message}");
        }
    }
}