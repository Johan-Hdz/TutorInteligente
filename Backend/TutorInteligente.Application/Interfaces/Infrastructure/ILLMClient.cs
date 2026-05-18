using System;
using System.Collections.Generic;
using System.Text;

namespace TutorInteligente.Application.Interfaces.Infrastructure
{
    public interface ILLMClient
    {
        Task<string> EjecutarPromptAsync(string prompt, float temperature);
    }
}
