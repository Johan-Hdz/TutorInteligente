using System.Text.Json;
using Microsoft.Extensions.AI;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class MotorGeneracionMeai(IChatClient chatClient) : IMotorGeneracion
{
    // Ahora recibimos el string con el ejemplo de texto
    public async Task<string> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto)
    {
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

        // Agregamos una regla en el prompt para obligarlo a usar el contexto
        string promptGraphRag = $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). 
Tu tarea es generar el contenido de una evaluación matemática rellenando un esqueleto JSON predefinido.

REGLAS ESTRICTAS:
1. Recibirás un JSON con la estructura de la evaluación. El tema principal a evaluar es '{esqueleto.TemaPrincipal}'.
2. EJEMPLO DE REFERENCIA: Usa el siguiente problema como guía de estilo, nivel de dificultad y contexto para generar los nuevos problemas: 
   ""{ejemploTexto}""
3. Para cada elemento en la lista de 'Preguntas', redacta un problema matemático práctico y contextualizado (apropiado para educación primaria) en el campo 'Enunciado'. BASATE EN EL EJEMPLO DE REFERENCIA.
4. Para cada 'Inciso' dentro de la pregunta, calcula el valor matemático correspondiente y ponlo en el campo 'ValorCalculado':
   - Si 'EsCorrecta' es true: El valor debe ser la respuesta matemáticamente correcta al problema.
   - Si 'EsCorrecta' es false: El valor debe ser INCORRECTO simulando una falla en el tema: 'TextoTema'.
5. El formato de salida debe ser ÚNICA Y EXCLUSIVAMENTE el JSON modificado y relleno. No agregues etiquetas markdown (como ```json).

ESQUELETO A RELLENAR:{jsonEsqueleto}";

        var opciones = new ChatOptions
        {
            Temperature = 0.2f
        };

        try
        {
            var mensajes = new[] { new ChatMessage(ChatRole.User, promptGraphRag) };
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);
            return respuesta.Text ?? "{}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Error interno en el LLM: {ex.Message}\"}}";
        }
    }
}