using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Neo4j.Driver;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Infrastructure.ServiciosLlm;

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
// 3. CREAMOS la variable (Aquí es donde nace el cliente y desaparece tu error)
IChatClient miClienteIa = new ChatClient("gpt-5.4-mini", apiKey).AsIChatClient();

// 3. CAMBIO DE CONTRATO: Quitamos el Mock y registramos la implementación de OpenAI
builder.Services.AddScoped<TutorInteligente.Application.Interfaces.IExtractorEntidades,
                           TutorInteligente.Infrastructure.ServiciosLlm.ExtractorEntidadesMeai>();

// Registro del contrato y la implementación mock
//builder.Services.AddScoped<TutorInteligente.Application.Interfaces.IExtractorEntidades,
//                           TutorInteligente.Infrastructure.ServiciosLlm.ExtractorEntidadesMock>();

// Registro del servicio de aplicación
builder.Services.AddScoped<TutorInteligente.Application.Servicios.ModuloInterpretacionService>();

#endregion

#region Configuracion de Neo4j
// Configuración de Neo4j
// --- CONFIGURACIÓN DE NEO4J ---
// Idealmente, estas credenciales vendrán de variables de entorno o Azure Key Vault,
// al igual que hiciste con las llaves de OpenAI/Gemini.
string neo4jUri = Environment.GetEnvironmentVariable("NEO4J_URI") ?? "bolt://localhost:7687";
string neo4jUser = Environment.GetEnvironmentVariable("NEO4J_USER") ?? "neo4j";
string neo4jPassword = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "2018134021";

// Se registra el Driver como Singleton
builder.Services.AddSingleton<IDriver>(provider =>
    GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword))
);

// Registro del repositorio (Scoping porque se crea uno por cada petición HTTP)
builder.Services.AddScoped<TutorInteligente.Application.Interfaces.IGrafoConocimientoRepository,
                           TutorInteligente.Infrastructure.Data.Neo4jGrafoRepository>();
#endregion

builder.Services.AddSingleton<IChatClient>(miClienteIa);

// Agregar el motor de generacion
builder.Services.AddScoped<IMotorGeneracion, MotorGeneracionMeai>();

// Registro del servicio orquestador
builder.Services.AddScoped<TutorInteligente.Application.Servicios.RecuperadorHibridoService>();

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

// configuración de CORS para permitir solicitudes desde el frontend (ajusta el origen según tu configuración)
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // El puerto por defecto de Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});



var app = builder.Build();

// Habilitar CORS
app.UseCors("PermitirFrontend");

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();

app.Run();
