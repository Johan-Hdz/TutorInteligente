using TutorInteligente.Application.Interfaces;
using Microsoft.Extensions.AI;
using TutorInteligente.Domain.Modelos;
using TutorInteligente.Application.Interfaces.Infrastructure;

namespace TutorInteligente.Application.Servicios;

public class RecuperadorHibridoService(IEmbeddingGenerator<string, Embedding<float>> embeddingGenerator, INeo4jService neo4jService) : IRecuperadorHibridoService
{
    public async Task<ContextoRecuperado> RecuperarContextoAsync(ParametrosEvaluacion parametros)
    {
        // 1. Generar el embedding del tema principal extraído
        // Nota: text-embedding-3-small genera un vector de 1536 dimensiones
        var embeddingResult = await embeddingGenerator.GenerateAsync(parametros.TemaPrincipal);
        var vectorTema = embeddingResult.Vector.ToArray();

        // 2. Realizar la búsqueda híbrida en Neo4j usando el vector
        var datosNeo4j = await neo4jService.ObtenerDatosCompletosTemaAsync(vectorTema);

        // 3. Mapear la tupla al modelo de dominio ContextoRecuperado
        var contextoGrafo = new ContextoRecuperado(
            Parametros: parametros,             // Pasas los parámetros que llegaron al método
            Subgrafo: datosNeo4j.Prerrequisitos, // Extraes la lista de la tupla
            EsExitoso: true,                    // Indicamos que la recuperación fue exitosa
            Mensaje: datosNeo4j.EjemploTexto    // Guardas el ejemplo del problema aquí
        );
        return contextoGrafo;
    }
}