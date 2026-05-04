using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorInteligente.Api.DTOs;
using TutorInteligente.Application.Servicios;

namespace TutorInteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class InterpretacionController(ModuloInterpretacionService interpretacionService) : ControllerBase
    {
        [HttpPost("procesar")]
        public async Task<IActionResult> ProcesarConsulta([FromBody] InterpretacionRequest request)
        {
            var resultado = await interpretacionService.ProcesarConsultaAsync(request.Consulta);

            if (!resultado.EsValida)
                return BadRequest(resultado);

            return Ok(resultado);
        }
    }
}
