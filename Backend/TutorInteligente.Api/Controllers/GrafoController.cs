using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TutorInteligente.Application.Interfaces;

namespace TutorInteligente.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class GrafoController(IGrafoConocimientoRepository grafoRepo) : ControllerBase
    {
        // Un endpoint GET simple para probar la extracción
        [HttpGet("prerrequisitos/{tema}")]
        public async Task<IActionResult> ObtenerContexto(string tema)
        {
            var subgrafo = await grafoRepo.ObtenerPrerrequisitosAsync(tema);

            if (subgrafo.Count == 0)
                return NotFound(new { Mensaje = $"No se encontró el tema '{tema}' o no tiene relaciones." });

            return Ok(subgrafo);
        }
    }
}
