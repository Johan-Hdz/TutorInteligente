using Microsoft.Extensions.AI;

namespace TutorInteligente.Application.Interfaces.Infrastructure
{
    public interface ILLMClient
    {
        // Cambiamos string prompt por IList<ChatMessage>
        Task<string> EjecutarPromptAsync(IList<ChatMessage> mensajes, float temperature);
    }
}
