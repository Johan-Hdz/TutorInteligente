using TutorInteligente.Application.Interfaces;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Servicios;

public class ModuloInterpretacionService(IExtractorEntidades extractor)
{
    public async Task<ParametrosEvaluacion> ProcesarConsultaAsync(string consultaDocente)
    {
        if (string.IsNullOrWhiteSpace(consultaDocente))
        {
            return new ParametrosEvaluacion("", 0, [], false, "La consulta está vacía.");
        }

        // Delega la extracción al LLM (a través de la interfaz)
        return await extractor.ExtraerAsync(consultaDocente);
    }
}
