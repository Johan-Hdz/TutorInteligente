using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces;

public interface IMotorGeneracion
{
    // Recibe el contexto completo del grafo y devuelve el texto de la evaluación
    Task<string> GenerarEvaluacionAsync(ContextoRecuperado contexto);
}
