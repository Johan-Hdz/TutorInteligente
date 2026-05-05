using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Servicios;

public class RecuperadorHibridoService(
    IExtractorEntidades extractor,
    IGrafoConocimientoRepository grafoRepo,
    IMotorGeneracion motorGeneracion)
{
    public async Task<ContextoRecuperado> ProcesarSolicitudAsync(string consultaDocente)
    {
        // 1. Extraer la intención
        var parametros = await extractor.ExtraerAsync(consultaDocente);

        if (!parametros.EsValida)
            return new ContextoRecuperado(parametros, [], false, parametros.MensajeError);

        // 2. Reglas de negocio
        if (parametros.GradoEscolar < 1 || parametros.GradoEscolar > 4)
            return new ContextoRecuperado(parametros, [], false, "Solo aplica para 1ro a 4to grado.");

        // 3. Navegar el grafo
        var subgrafo = await grafoRepo.ObtenerPrerrequisitosAsync(parametros.TemaPrincipal);

        if (subgrafo.Count == 0)
            return new ContextoRecuperado(parametros, [], false, $"No se encontró el tema '{parametros.TemaPrincipal}'.");

        // 4. Armar el contexto temporal para enviarlo al motor
        var contextoTemporal = new ContextoRecuperado(parametros, subgrafo, true, "Generando evaluación...");

        // 5. Llamada final al Motor de Generación
        string evaluacionFinal = await motorGeneracion.GenerarEvaluacionAsync(contextoTemporal);

        // 6. Retornar el objeto COMPLETO: Intención + Grafo + Evaluación final
        return new ContextoRecuperado(
            parametros,
            subgrafo,
            true,
            "Evaluación generada con éxito mediante GraphRAG.",
            evaluacionFinal
        );
    }
}
