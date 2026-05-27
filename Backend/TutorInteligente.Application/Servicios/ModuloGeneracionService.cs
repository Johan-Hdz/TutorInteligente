using System.Text.Json;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Servicios;

public class ModuloGeneracionService(ILLMClient llmClient) : IModuloGeneracionService
{
    public async Task<string> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto)
    {
        // 1. Lógica pura de aplicación: Preparación de datos
        var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
        string jsonEsqueleto = JsonSerializer.Serialize(esqueleto, jsonOptions);

        // 2. Lógica de negocio/dominio: El Prompt pedagógico
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

        // 3. Delegar la ejecución a la capa de infraestructura
        return await llmClient.EjecutarPromptAsync(promptGraphRag, temperature: 0.0f);
    }
}