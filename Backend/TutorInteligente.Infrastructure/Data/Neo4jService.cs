using Neo4j.Driver;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos; // Asegúrate de que aquí esté TemaJerarquico

namespace TutorInteligente.Infrastructure.Data;

public class Neo4jService(IDriver driver) : INeo4jService
{
    public async Task<List<TemaJerarquico>> ObtenerPrerrequisitosAsync(string temaPrincipal)
    {
        var query = @"
            MATCH (t:Tema {nombre: $tema})-[:REQUIERE_DE]->(p:Tema)
            RETURN p.nombre AS prerrequisito";

        await using var session = driver.AsyncSession();
        var result = await session.RunAsync(query, new { tema = temaPrincipal });

        var prerrequisitos = new List<TemaJerarquico>();
        await foreach (var record in result)
        {
            // Pasamos los valores directamente al constructor entre paréntesis
            prerrequisitos.Add(new TemaJerarquico(
                record["prerrequisito"].As<string>(),
                true
            ));
        }

        return prerrequisitos;
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