using Neo4j.Driver;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.Data;

public class Neo4jGrafoRepository(IDriver driver) : IGrafoConocimientoRepository
{
    public async Task<List<TemaJerarquico>> ObtenerPrerrequisitosAsync(string temaPrincipal)
    {
        var temasContexto = new List<TemaJerarquico>();

        // Las sesiones asíncronas son la mejor práctica en Neo4j para el manejo de recursos
        await using var session = driver.AsyncSession();

        // Consulta Cypher basada en tu diagrama:
        // Buscamos el nodo central, y todos los nodos que apuntan a él con REQUISITO_DE
        var query = @"
            MATCH (temaDestino:Tema {nombre: $tema})<-[:REQUISITO_DE]-(prerrequisito:Tema)
            RETURN prerrequisito.nombre AS NombrePrerrequisito
        ";

        var parametros = new { tema = temaPrincipal };

        try
        {
            var cursor = await session.RunAsync(query, parametros);

            // 1. Agregamos el tema central a nuestra lista de contexto
            temasContexto.Add(new TemaJerarquico(temaPrincipal, EsPrerrequisito: false));

            // 2. Iteramos sobre los resultados y agregamos los prerrequisitos
            await cursor.ForEachAsync(record =>
            {
                var nombreReq = record["NombrePrerrequisito"].As<string>();
                temasContexto.Add(new TemaJerarquico(nombreReq, EsPrerrequisito: true));
            });

            return temasContexto;
        }
        catch (Exception ex)
        {
            // En un entorno real, aquí usarías ILogger
            Console.WriteLine($"Error al consultar Neo4j: {ex.Message}");
            return [];
        }
    }
}