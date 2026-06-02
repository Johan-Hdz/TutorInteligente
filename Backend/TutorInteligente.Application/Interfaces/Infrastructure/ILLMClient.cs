using Microsoft.Extensions.AI;

namespace TutorInteligente.Application.Interfaces.Infrastructure
{
    public interface ILLMClient
    {
        Task<string> EjecutarPromptAsync(IList<ChatMessage> mensajes, float temperature);
    }
}
