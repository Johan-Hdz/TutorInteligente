using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces;

public interface IModuloGeneracionService
{
    Task<Evaluacion> GenerarEvaluacionAsync(Evaluacion esqueleto, string ejemploTexto);
}