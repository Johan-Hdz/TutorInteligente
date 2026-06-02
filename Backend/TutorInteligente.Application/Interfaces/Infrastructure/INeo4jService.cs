using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces.Infrastructure;  

public interface INeo4jService
{
    Task<(List<TemaJerarquico> Prerrequisitos, string EjemploTexto)> ObtenerDatosCompletosTemaAsync(float[] temaPrincipalEmbedding);
}