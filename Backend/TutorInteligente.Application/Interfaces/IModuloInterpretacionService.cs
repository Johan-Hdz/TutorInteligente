using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces
{
    public interface IModuloInterpretacionService
    {
        Task<ParametrosEvaluacion> ProcesarConsultaAsync(string consulta);
    }
}
