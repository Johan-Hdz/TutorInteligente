using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Servicios;

public class RecuperadorHibridoService(
    IExtractorEntidades extractor,
    INeo4jService grafoRepo,
    IEvaluacionGeneratorService generadorEstructura, // <-- 1. Inyectamos nuestro generador de permutaciones
    IMotorGeneracion motorGeneracion)
{
    public async Task<ContextoRecuperado> ProcesarSolicitudAsync(string consultaDocente)
    {
        // 1. Extraer la intención
        var parametros = await extractor.ExtraerAsync(consultaDocente);

        if (!parametros.EsValida)
            return new ContextoRecuperado(parametros, [], false, parametros.MensajeError);

        // 2. Reglas de negocio (comentadas por ahora como las tenías)
        //if (parametros.GradoEscolar < 1 || parametros.GradoEscolar > 4)
        //    return new ContextoRecuperado(parametros, [], false, "Solo aplica para 1ro a 4to grado.");

        // 3. Navegar el grafo (Recuperación de prerrequisitos / distractores)
        var subgrafo = await grafoRepo.ObtenerPrerrequisitosAsync(parametros.TemaPrincipal);

        if (subgrafo.Count == 0)
            return new ContextoRecuperado(parametros, [], false, $"No se encontró el tema '{parametros.TemaPrincipal}'.");

        // 4. Generar la estructura matemática (Conmutación y distribución de incisos)
        // Asumimos que "parametros" tiene la cantidad de preguntas solicitada. Si no, ponemos 5 por defecto.
        int cantidadPreguntas = parametros.cantidadPreguntas > 0 ? parametros.cantidadPreguntas : 5;

        Evaluacion esqueletoEvaluacion = generadorEstructura.GenerarEstructura(
            parametros.TemaPrincipal,
            subgrafo.Select(t => t.Nombre).ToList(),
            cantidadPreguntas
        );

        // 5. Armar el contexto temporal (Opcional, dependiendo de si IMotorGeneracion lo requiere)
        // Aquí puedes guardar el esqueleto dentro de tu ContextoRecuperado si tu motor lo extrae de ahí
        var contextoTemporal = new ContextoRecuperado(parametros, subgrafo, true, "Generando evaluación...");

        // 6. Llamada final al Motor de Generación (LLM)
        // Ahora le pasamos el esqueleto determinista para que solo se encargue de rellenar los huecos
        string evaluacionFinal = await motorGeneracion.GenerarEvaluacionAsync(esqueletoEvaluacion);

        // 7. Retornar el objeto COMPLETO
        return new ContextoRecuperado(
            parametros,
            subgrafo,
            true,
            "Evaluación generada con éxito mediante GraphRAG y distribución dinámica.",
            evaluacionFinal
        );
    }
}