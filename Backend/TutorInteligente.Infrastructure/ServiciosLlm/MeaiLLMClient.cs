using Microsoft.Extensions.AI;
using System;
using System.Collections.Generic;
using System.Text;
using TutorInteligente.Application.Interfaces.Infrastructure;

namespace TutorInteligente.Infrastructure.ServiciosLlm;

public class MeaiLLMClient(IChatClient chatClient) : ILLMClient
{
    public async Task<string> EjecutarPromptAsync(string prompt, float temperature)
    {
        var opciones = new ChatOptions { Temperature = temperature };

        try
        {
            var mensajes = new[] { new ChatMessage(ChatRole.User, prompt) };
            var respuesta = await chatClient.GetResponseAsync(mensajes, opciones);
            return respuesta.Text ?? "{}";
        }
        catch (Exception ex)
        {
            // El manejo de errores de conexión/API va aquí
            return $"{{\"error\": \"Error interno en la API del LLM: {ex.Message}\"}}";
        }
    }
}
