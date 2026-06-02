using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces;

public interface IRecuperadorHibridoService
{
    Task<ContextoRecuperado> RecuperarContextoAsync(ParametrosEvaluacion parametros);
}