using Neo4j.Driver;
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.Data;

public class Neo4jService(IDriver driver) : INeo4jService
{
    public async Task<(List<TemaJerarquico> Prerrequisitos, string EjemploTexto)> ObtenerDatosCompletosTemaAsync(float[] temaPrincipalEmbedding)
    {
        // Usamos búsqueda vectorial (db.index.vector.queryNodes) para encontrar el nodo más similar
        var query = @"
    CALL db.index.vector.queryNodes('nombre_tema_embedding_index', 1, $tema)
    YIELD node AS t, score
    WHERE score >= 0.85
    OPTIONAL MATCH (t)-[:REQUIERE_DE]->(p:Tema)
    RETURN
        t.problemaTexto AS ejemplo,
        collect({
            nombreTema: p.nombreTema,
            modeloMatematico: p.modeloMatematico
        }) AS prerrequisitos,
        score";

        // 0.85 es una metrica de confianza y pueden preguntar por que

        await using var session = driver.AsyncSession();

        // Le pasamos el embedding convertido a List<float> (Neo4j lo prefiere así)
        var result = await session.RunAsync(query, new { tema = temaPrincipalEmbedding.ToList() });

        var prerrequisitos = new List<TemaJerarquico>();
        string ejemploTexto = "";

        if (await result.FetchAsync())
        {
            var ejemploValue = result.Current["ejemplo"];
            if (ejemploValue != null)
            {
                ejemploTexto = ejemploValue.As<string>();
            }

            var prerrequisitosCypher =
                result.Current["prerrequisitos"]
                      .As<List<Dictionary<string, object>>>();

            foreach (var prerrequisito in prerrequisitosCypher)
            {
                var nombreTema = prerrequisito["nombreTema"]?.ToString();
                var modeloMatematico = prerrequisito["modeloMatematico"]?.ToString();

                if (!string.IsNullOrWhiteSpace(nombreTema))
                {
                    prerrequisitos.Add(
                        new TemaJerarquico(
                            nombreTema,
                            modeloMatematico ?? string.Empty,
                            true
                        )
                    );
                }
            }
        }

        return (prerrequisitos, ejemploTexto);
    }
}