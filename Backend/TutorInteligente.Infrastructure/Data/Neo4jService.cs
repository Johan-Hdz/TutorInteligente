using Neo4j.Driver;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos; // Asegúrate de que aquí esté TemaJerarquico

namespace TutorInteligente.Infrastructure.Data;

public class Neo4jService(IDriver driver) : INeo4jService
{
   public async Task<(List<TemaJerarquico> Prerrequisitos, string EjemploTexto)> ObtenerDatosCompletosTemaAsync(string temaPrincipal)
{
    // Cambiamos t.embedding por la propiedad que guarda el texto (ej. t.ejemplo_problema)
    var query = @"
        MATCH (t:Tema {nombre: $tema})
        OPTIONAL MATCH (t)-[:REQUIERE_DE]->(p:Tema)
        RETURN t.ejemplo_problema AS ejemplo, collect(p.nombre) AS prerrequisitos
    ";

    await using var session = driver.AsyncSession();
    var result = await session.RunAsync(query, new { tema = temaPrincipal });

    var prerrequisitos = new List<TemaJerarquico>();
    string ejemploTexto = ""; // Ahora es un string

    if (await result.FetchAsync())
    {
        // 1. Extraemos el texto del ejemplo
        var ejemploValue = result.Current["ejemplo"];
        if (ejemploValue != null)
        {
            ejemploTexto = ejemploValue.As<string>();
        }

        // 2. Extraer la lista de prerrequisitos
        var nombresPrerrequisitos = result.Current["prerrequisitos"].As<List<string>>();
        foreach (var nombre in nombresPrerrequisitos)
        {
            if (!string.IsNullOrEmpty(nombre))
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