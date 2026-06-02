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
            CALL db.index.vector.queryNodes('nombre_tema_embedding_index', 1, $tema) YIELD node AS t, score
            WHERE score >= 0.85
            OPTIONAL MATCH (t)-[:REQUIERE_DE]->(p:Tema)
            RETURN t.problemaTexto AS ejemplo, collect(p.nombreTema) AS prerrequisitos, score";

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

            var nombresPrerrequisitos = result.Current["prerrequisitos"].As<List<string>>();
            foreach (var nombre in nombresPrerrequisitos)
            {
                if (!string.IsNullOrWhiteSpace(nombre))
                {
                    prerrequisitos.Add(new TemaJerarquico(nombre, true));
                }
            }
        }

        return (prerrequisitos, ejemploTexto);
    }
}