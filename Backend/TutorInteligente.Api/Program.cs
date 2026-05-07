using Microsoft.Extensions.AI;
using OpenAI.Chat;
using Neo4j.Driver;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Infrastructure.ServiciosLlm;
using TutorInteligente.Application.Servicios;
using TutorInteligente.Infrastructure.Data;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Configuración de OpenAI
// --- CONFIGURACIÓN DE OPENAI ---
// 1. Obtener la clave desde las variables de entorno de Windows. 
// (Asegúrate de que el nombre de la variable aquí coincida exactamente con cómo la llamaste en Windows, usualmente es OPENAI_API_KEY)
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("La variable de entorno OPENAI_API_KEY no se encontró en Windows.");

// Indicar el modelo y la clave al crear el ChatClient de OpenAI.
// 2. CREAMOS la variable
IChatClient miClienteIa = new ChatClient("gpt-4o-mini", apiKey).AsIChatClient();

// 3. Registrar el ChatClient de OpenAI como Singleton (solo necesitamos uno para toda la app)
builder.Services.AddSingleton<IChatClient>(miClienteIa);
#endregion

#region Configuracion de Neo4j
// --- CONFIGURACIÓN DE NEO4J ---
// Idealmente, estas credenciales vendrán de variables de entorno o Azure Key Vault,
// al igual que hiciste con las llaves de OpenAI.
string neo4jUri = Environment.GetEnvironmentVariable("NEO4J_URI") ?? "bolt://localhost:7687";
string neo4jUser = Environment.GetEnvironmentVariable("NEO4J_USER") ?? "neo4j";
string neo4jPassword = Environment.GetEnvironmentVariable("NEO4J_PASSWORD") ?? "2018134021";

// Se registra el Driver como Singleton
builder.Services.AddSingleton<IDriver>(provider =>
    GraphDatabase.Driver(neo4jUri, AuthTokens.Basic(neo4jUser, neo4jPassword))
);

// Registro del repositorio (Scoping porque se crea uno por cada petición HTTP)
builder.Services.AddScoped<INeo4jService, Neo4jService>();
#endregion

#region Configuracion de los contratos y servicios de la aplicación
// Quitamos el Mock y registramos la implementación de OpenAI con Microsoft.Extensions.AI
builder.Services.AddScoped<IExtractorEntidades, ExtractorEntidadesMeai>();

// Registro del servicio de aplicación
builder.Services.AddScoped<ModuloInterpretacionService>();

// Agregar el motor de generacion
builder.Services.AddScoped<IMotorGeneracion, MotorGeneracionMeai>();

// Registro del servicio orquestador (El que maneja el flujo de GraphRAG)
builder.Services.AddScoped<RecuperadorHibridoService>();

// --- NUEVO: Registro del Generador de Evaluaciones Estructuradas ---
builder.Services.AddScoped<IEvaluacionGeneratorService, EvaluacionGeneratorService>();
#endregion

#region Configuración de CORS
// configuración de CORS para permitir solicitudes desde el frontend
builder.Services.AddCors(options =>
{
    options.AddPolicy("PermitirFrontend", policy =>
    {
        policy.WithOrigins("http://localhost:5173") // El puerto por defecto de Vite
              .AllowAnyHeader()
              .AllowAnyMethod();
    });
});
#endregion

#region configuración por default del proyecto, sin cambios
builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

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
#endregion