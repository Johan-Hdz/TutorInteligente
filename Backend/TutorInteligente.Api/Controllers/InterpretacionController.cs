using Microsoft.AspNetCore.Mvc;
using TutorInteligente.Api.DTOs;
using TutorInteligente.Application.Interfaces;

namespace TutorInteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterpretacionController(IModuloInterpretacionService interpretacionService,
        IRecuperadorHibridoService recuperadorHibridoService,
        IGenerarEstructuraEvaluacion evaluacionGeneratorService,
        IModuloGeneracionService motorGeneracionMeai) : ControllerBase
    {
        [HttpPost("orquestar")]
        public async Task<IActionResult> OrquestarGraphRAG([FromBody] InterpretacionRequest request)
        {
            // PASO 1: Interpretación de la Consulta
            var parametros = await interpretacionService.ProcesarConsultaAsync(request.Consulta);

            // Validación de los parámetros extraídos
            if (!parametros.EsValida)
                return BadRequest(parametros);

            // PASO 2: Recuperación Híbrida (Embedding + Neo4j)
            // Pasamos el objeto completo porque el recuperador podría necesitar el GradoEscolar para filtrar
            var contextoRecuperado = await recuperadorHibridoService.RecuperarContextoAsync(parametros);

            if (contextoRecuperado == null || !contextoRecuperado.Subgrafo.Any())
            {
                return NotFound(new { Error = "No se encontraron temas afines en la base de conocimiento para tu solicitud." });
            }

            // PASO 3: Ensamblaje del Esqueleto (Motor de Generación - Fase 1)

            // a) Extraemos solo los nombres de los prerrequisitos del Subgrafo recuperado
            var nombresPrerrequisitos = contextoRecuperado.Subgrafo
                                                          .Select(p => p.Nombre)
                                                          .ToList();
            // b) Generamos la estructura base determinista
            var esqueletoEvaluacion = evaluacionGeneratorService.GenerarEstructura(
                temaPrincipal: parametros.TemaPrincipal,
                prerrequisitos: nombresPrerrequisitos,
                cantidadPreguntas: parametros.cantidadPreguntas
            );


            // PASO 4 y 5: Motor de Generación (Incluye Validación y Auto-Corrección interna)
            try
            {
                var evaluacionValidada = await motorGeneracionMeai.GenerarEvaluacionAsync(esqueletoEvaluacion, contextoRecuperado.Mensaje);

                // EMPAQUETAMOS TODO PARA REACT
                return Ok(new
                {
                    temaPrincipal = parametros.TemaPrincipal,
                  
                    subgrafo = contextoRecuperado.Subgrafo,
                    evaluacion = evaluacionValidada
                });
            }
            catch (InvalidOperationException ex)
            {
                return UnprocessableEntity(new
                {
                    Error = "No se pudo generar una evaluación matemáticamente válida tras varios intentos.",
                    Detalles = ex.Message
                });
            }
        }
    }
}
