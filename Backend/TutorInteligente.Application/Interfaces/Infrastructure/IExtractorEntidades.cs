using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces.Infrastructure
{
    public interface IExtractorEntidades
    {
        Task<ParametrosEvaluacion> ExtraerAsync(string consultaDocente);
    }
}
