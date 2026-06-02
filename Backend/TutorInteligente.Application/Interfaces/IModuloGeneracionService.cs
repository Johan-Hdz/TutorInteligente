using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces
{
    public interface IModuloGeneracionService
    {
        Task<Evaluacion> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto);
    }
}
