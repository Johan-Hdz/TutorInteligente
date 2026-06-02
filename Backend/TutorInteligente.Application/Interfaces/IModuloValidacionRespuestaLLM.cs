using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces;

public interface IModuloValidacionRespuestaLLM
{
    Task<(bool EsValido, List<string> Errores, Evaluacion? EvaluacionValidada)> ValidarRespuestaLlm(string jsonLlmOutput);
}