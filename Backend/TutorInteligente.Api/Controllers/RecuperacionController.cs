using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorInteligente.Api.DTOs;
using TutorInteligente.Application.Servicios;

namespace TutorInteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class RecuperacionController(RecuperadorHibridoService recuperadorService) : ControllerBase
    {
        [HttpPost("orquestar")]
        public async Task<IActionResult> OrquestarGraphRag([FromBody] InterpretacionRequest request)
        {
            var resultado = await recuperadorService.ProcesarSolicitudAsync(request.Consulta);

            if (!resultado.EsExitoso)
            {
                return BadRequest(new { Error = resultado.Mensaje, Detalles = resultado.Parametros });
            }

            return Ok(resultado);
        }
    }
}
