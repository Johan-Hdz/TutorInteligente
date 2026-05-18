using Neo4j.Driver;
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos; // Asegúrate de que aquí esté TemaJerarquico

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
    RETURN t.ejemplo_problema AS ejemplo, collect(p.nombre) AS prerrequisitos, score";

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

//public class Neo4jService(IDriver driver) : IGrafoConocimientoRepository
//{
//    public async Task<List<TemaJerarquico>> ObtenerPrerrequisitosAsync(string temaPrincipal)
//    {
//        var temasContexto = new List<TemaJerarquico>();

//        // Las sesiones asíncronas son la mejor práctica en Neo4j para el manejo de recursos
//        await using var session = driver.AsyncSession();

//        // Consulta Cypher basada en tu diagrama:
//        // Buscamos el nodo central, y todos los nodos que apuntan a él con REQUISITO_DE
//        var query = @"
//            MATCH (cDestino:Tema {nombre: $tema})
//            OPTIONAL MATCH (cDestino)-[:REQUIERE_DE]->(prerrequisito:Tema)
//            RETURN cDestino.nombre AS TemaPrincipal, prerrequisito.nombre AS NombrePrerrequisito
//        ";

//        var parametros = new { tema = temaPrincipal };

//        try
//        {
//            var cursor = await session.RunAsync(query, parametros);

//            // 1. Agregamos el tema central a nuestra lista de contexto
//            temasContexto.Add(new TemaJerarquico(temaPrincipal, EsPrerrequisito: false));

//            // 2. Iteramos sobre los resultados y agregamos los prerrequisitos
//            await cursor.ForEachAsync(record =>
//            {
//                var nombreReq = record["NombrePrerrequisito"].As<string>();
//                temasContexto.Add(new TemaJerarquico(nombreReq, EsPrerrequisito: true));
//            });

//            return temasContexto;
//        }
//        catch (Exception ex)
//        {
//            // En un entorno real, aquí usarías ILogger
//            Console.WriteLine($"Error al consultar Neo4j: {ex.Message}");
//            return [];
//        }
//    }
//}