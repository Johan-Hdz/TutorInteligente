using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Application.Interfaces.Infrastructure;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class MeaiLLMClient(IChatClient chatClient) : ILLMClient
{
    public async Task<string> EjecutarPromptAsync(IList<ChatMessage> mensajes, float temperature)
    {
        var opciones = new ChatOptions { Temperature = temperature };

        try
        {
            // Ya no creamos el arreglo aquí, pasamos el historial completo
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);
            return respuesta.Text ?? "{}";
        }
        catch (Exception ex)
        {
            return $"{{\"error\": \"Error interno en la API del LLM: {ex.Message}\"}}";
        }
    }
}
