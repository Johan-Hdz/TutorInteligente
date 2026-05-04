using System;
using System.Collections.Generic;
using System.Text;

namespace TutorInteligente.Domain.Modelos
{
    public record ParametrosEvaluacion(

        string TemaPrincipal,
        int GradoEscolar,
        List<string> ConceptosClave,
        bool EsValida,
        string MensajeError = ""
    );
}
