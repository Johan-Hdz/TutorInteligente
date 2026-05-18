using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Domain.Modelos;

namespace TutorInteligente.Application.Interfaces
{
    public interface IRecuperadorHibridoService
    {
        Task<ContextoRecuperado> RecuperarContextoAsync(ParametrosEvaluacion parametros);
    }
}
