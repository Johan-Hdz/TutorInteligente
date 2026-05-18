using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces
{
    public interface IGenerarEstructuraEvaluacion
    {
        Evaluacion GenerarEstructura(string temaPrincipal, List<string> prerrequisitos, int cantidadPreguntas);
    }
}
