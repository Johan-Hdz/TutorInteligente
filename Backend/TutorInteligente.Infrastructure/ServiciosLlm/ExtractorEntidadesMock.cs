using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class ExtractorEntidadesMock : IExtractorEntidades
{
    public async Task<ParametrosEvaluacion> ExtraerAsync(string consultaDocente)
    {
        // Simulamos el retraso de una llamada a una API de IA
        await Task.Delay(800);

        var texto = consultaDocente.ToLower();

        // Extracción rudimentaria simulando la inteligencia del LLM
        int grado = texto.Contains("cuarto") || texto.Contains("4to") ? 4 :
                    texto.Contains("tercero") || texto.Contains("3ro") ? 3 : 1;

        string tema = texto.Contains("fracciones") ? "Fracciones" :
                      texto.Contains("sumas") ? "Sumas" : "Tema general";

        var conceptos = new List<string>();
        if (tema == "Fracciones")
        {
            conceptos.Add("Numerador");
            conceptos.Add("Denominador");
        }

        return new ParametrosEvaluacion(tema, 0, true);
    }
}