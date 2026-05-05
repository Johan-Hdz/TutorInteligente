using System;
using System.Collections.Generic;
using System.Text;

namespace TutorInteligente.Domain.Modelos
{
    public record TemaJerarquico(
     string Nombre,
     bool EsPrerrequisito // Nos ayudará a distinguir el tema central de sus bases
 );
}
