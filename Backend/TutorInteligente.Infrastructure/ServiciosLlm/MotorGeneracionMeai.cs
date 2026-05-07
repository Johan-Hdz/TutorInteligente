using System.Text.Json;
using Microsoft.Extensions.AI;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class MotorGeneracionMeai(IChatClient chatClient) : IMotorGeneracion
{
    // Cambié el nombre del parámetro de 'contexto' a 'esqueleto' para mayor claridad
    public async Task<string> GenerarEvaluacionAsync(Evaluacion esqueleto)
    {
        // 1. Convertimos el esqueleto determinista a un JSON ligero para que el LLM lo lea
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

        // 2. Construimos el nuevo prompt. Ahora le exigimos que devuelva JSON.
        string promptGraphRag = $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). 
Tu tarea es generar el contenido de una evaluación matemática rellenando un esqueleto JSON predefinido.

REGLAS ESTRICTAS:
1. Recibirás un JSON con la estructura de la evaluación. El tema principal a evaluar es '{esqueleto.TemaPrincipal}'.
2. Para cada elemento en la lista de 'Preguntas', redacta un problema matemático práctico y contextualizado (apropiado para educación primaria) en el campo 'Enunciado'.
3. Para cada 'Inciso' dentro de la pregunta, calcula el valor matemático correspondiente y ponlo en el campo 'ValorCalculado':
   - Si 'EsCorrecta' es true: El valor debe ser la respuesta matemáticamente correcta al problema.
   - Si 'EsCorrecta' es false: El valor debe ser INCORRECTO. Para generar este distractor, debes simular que el alumno intentó resolver el problema pero falló específicamente en el tema indicado en 'TextoTema' (que es un prerrequisito cognitivo).
4. El formato de salida debe ser ÚNICA Y EXCLUSIVAMENTE el JSON modificado y relleno. No agregues etiquetas markdown (como ```json), no saludes, ni des explicaciones.

ESQUELETO A RELLENAR:
{jsonEsqueleto}";

        // 3. Mantenemos la temperatura baja para evitar alucinaciones en los cálculos
        var opciones = new ChatOptions
        {
            Temperature = 0.2f
        };

        try
        {
            var mensajes = new[]
            {
                new ChatMessage(ChatRole.User, promptGraphRag)
            };

            // Llamada al modelo con Microsoft.Extensions.AI
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);

            // Retornamos el string crudo (que ahora será un JSON estructurado)
            return respuesta.Text ?? "{}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Error interno en el LLM: {ex.Message}\"}}";
        }
    }
}