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

//public class RecuperadorHibridoService(
//    IExtractorEntidades extractor,
//    INeo4jService grafoRepo,
//    IEvaluacionGeneratorService generadorEstructura, // <-- 1. Inyectamos nuestro generador de permutaciones
//    IMotorGeneracion motorGeneracion)
//{
//    public async Task<ContextoRecuperado> ProcesarSolicitudAsync(string consultaDocente)
//    {
//        // 1. Extraer la intención
//        var parametros = await extractor.ExtraerAsync(consultaDocente);

//        if (!parametros.EsValida)
//            return new ContextoRecuperado(parametros, [], false, parametros.MensajeError);

//        // 2. Reglas de negocio (comentadas por ahora como las tenías)
//        //if (parametros.GradoEscolar < 1 || parametros.GradoEscolar > 4)
//        //    return new ContextoRecuperado(parametros, [], false, "Solo aplica para 1ro a 4to grado.");

//        // 3. Navegar el grafo (Recuperación de prerrequisitos / distractores)
//        var (subgrafo, ejemploTexto) = await grafoRepo.ObtenerDatosCompletosTemaAsync(parametros.TemaPrincipal);

//        if (subgrafo.Count == 0)
//            return new ContextoRecuperado(parametros, [], false, $"No se encontró el tema '{parametros.TemaPrincipal}'.");

//        // 4. Generar la estructura matemática (Conmutación y distribución de incisos)
//        // Asumimos que "parametros" tiene la cantidad de preguntas solicitada. Si no, ponemos 5 por defecto.
//        int cantidadPreguntas = parametros.cantidadPreguntas > 0 ? parametros.cantidadPreguntas : 5;

//        Evaluacion esqueletoEvaluacion = generadorEstructura.GenerarEstructura(
//            parametros.TemaPrincipal,
//            subgrafo.Select(t => t.Nombre).ToList(),
//            cantidadPreguntas
//        );

//        // 5. Armar el contexto temporal (Opcional, dependiendo de si IMotorGeneracion lo requiere)
//        // Aquí puedes guardar el esqueleto dentro de tu ContextoRecuperado si tu motor lo extrae de ahí
//        var contextoTemporal = new ContextoRecuperado(parametros, subgrafo, true, "Generando evaluación...");

//        // 6. Llamada final al Motor de Generación (LLM)
//        // Ahora le pasamos el esqueleto determinista para que solo se encargue de rellenar los huecos
//        string evaluacionFinal = await motorGeneracion.GenerarEvaluacionAsync(esqueletoEvaluacion, ejemploTexto);

//        // 7. Retornar el objeto COMPLETO
//        return new ContextoRecuperado(
//            parametros,
//            subgrafo,
//            true,
//            "Evaluación generada con éxito mediante GraphRAG y distribución dinámica.",
//            evaluacionFinal
//        );
//    }
//}