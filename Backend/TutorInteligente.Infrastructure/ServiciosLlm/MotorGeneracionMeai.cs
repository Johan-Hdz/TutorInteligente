using Microsoft.Extensions.AI;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class MotorGeneracionMeai(IChatClient chatClient) : IMotorGeneracion
{
    public async Task<string> GenerarEvaluacionAsync(ContextoRecuperado contexto)
    {
        // 1. Extraemos solo los nombres de los temas que son prerrequisitos
        var nombresPrerrequisitos = contexto.Subgrafo
            .Where(nodo => nodo.EsPrerrequisito)
            .Select(nodo => nodo.Nombre);
            
        string listaPrerrequisitos = string.Join(", ", nombresPrerrequisitos);

        // 2. Construimos el prompt interpolado con los datos de Neo4j y del Módulo de Interpretación
        string promptGraphRag = $@"Actúa como un Experto en Pedagogía Matemática y Diseño Curricular para la SEP (Nueva Escuela Mexicana). 
Tu tarea es generar un ítem de evaluación diagnóstica basado en un esquema de conocimientos previos.

CONTEXTO CURRICULAR EXTRAÍDO DEL GRAFO DE CONOCIMIENTO:
- Tema Principal a evaluar: {contexto.Parametros.TemaPrincipal}
- Nivel Educativo: {contexto.Parametros.GradoEscolar} grado de primaria.
- Prerrequisitos cognitivos del alumno para este tema: {listaPrerrequisitos}

INSTRUCCIONES:
1. Genera 1 ejercicio práctico (problema matemático) apropiado para el grado escolar que evalúe la comprensión del 'Tema Principal'.
2. Diseña 3 opciones de respuesta (A, B, C) siguiendo ESTRICTAMENTE esta lógica de generación de errores:
   - OPCIÓN A (Correcta): El resultado matemático exacto siguiendo el procedimiento correcto.
   - OPCIÓN B (Distractor por Prerrequisito): Simula que el alumno cometió un error operando en uno de los 'Prerrequisitos cognitivos' (ej. falló en {listaPrerrequisitos}).
   - OPCIÓN C (Distractor Conceptual): Simula que el alumno no entendió el concepto lógico del tema central.

FORMATO DE SALIDA REQUERIDO:
---
**Enunciado del Problema:** [Pregunta]

**Opciones:**
a) [Respuesta A]
b) [Respuesta B]
c) [Respuesta C]

**Metadatos para el Tutor Inteligente (Backend):**
* **Si elige A:** Estado: Dominio del tema. Acción: Avanzar.
* **Si elige B:** Tipo de Error: Fallo en Prerrequisito. Diagnóstico: Falla en [Prerrequisito específico]. Recomendación: Repasar prerrequisito.
* **Si elige C:** Tipo de Error: Conceptual. Diagnóstico: Idea errónea sobre el tema principal. Recomendación: Volver a explicar la teoría base.
---";

        var opciones = new ChatOptions { Temperature = 0.3f }; // Poca temperatura para mantener la lógica estricta

        try
        {
            var mensajes = new[]
            {
        new ChatMessage(ChatRole.User, promptGraphRag)
    };

            // Correcto para la versión Preview que tienes instalada
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);
            return respuesta.Text ?? "Error: No se pudo generar la evaluación.";
        }
        catch (Exception ex)
        {
            return $"Error interno en el LLM: {ex.Message}";
        }
    }
}