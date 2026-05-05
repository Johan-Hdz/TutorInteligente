using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces
{
    public interface IGrafoConocimientoRepository
    {
        // Recibe el tema extraído por el LLM y devuelve el subgrafo
        Task<List<TemaJerarquico>> ObtenerPrerrequisitosAsync(string temaPrincipal);
    }
}
