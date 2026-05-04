using OpenAI.Chat;
var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Configuración de OpenAI
// --- CONFIGURACIÓN DE OPENAI ---
// 1. Obtener la clave desde las variables de entorno de Windows. 
// (Asegúrate de que el nombre de la variable aquí coincida exactamente con cómo la llamaste en Windows, usualmente es OPENAI_API_KEY)
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("La variable de entorno OPENAI_API_KEY no se encontró en Windows.");

// 2. Registrar el ChatClient de OpenAI como Singleton (solo necesitamos uno para toda la app)
// Pasamos el modelo que indicaste y tu clave
builder.Services.AddSingleton(new ChatClient("gpt-5.4-mini", apiKey));

// 3. CAMBIO DE CONTRATO: Quitamos el Mock y registramos la implementación de OpenAI
builder.Services.AddScoped<TutorInteligente.Application.Interfaces.IExtractorEntidades,
                           TutorInteligente.Infrastructure.ServiciosLlm.ExtractorEntidadesOpenAi>();

// Registro del contrato y la implementación mock
//builder.Services.AddScoped<TutorInteligente.Application.Interfaces.IExtractorEntidades,
//                           TutorInteligente.Infrastructure.ServiciosLlm.ExtractorEntidadesMock>();

// Registro del servicio de aplicación
builder.Services.AddScoped<TutorInteligente.Application.Servicios.ModuloInterpretacionService>();

#endregion

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();


var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
