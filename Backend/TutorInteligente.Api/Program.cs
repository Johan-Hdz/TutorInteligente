using Microsoft.Extensions.AI;
using Neo4j.Driver;
using OpenAI.Chat;
using OpenAI.Embeddings;
using TutorInteligente.Application.Interfaces;
using TutorInteligente.Application.Interfaces.Infrastructure;
using TutorInteligente.Application.Servicios;
using TutorInteligente.Infrastructure.Data;
using TutorInteligente.Infrastructure.ServiciosLlm;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

#region Configuración de OpenAI
// --- CONFIGURACIÓN DE OPENAI ---
// 1. Obtener la clave desde las variables de entorno de Windows. 
// (Asegúrate de que el nombre de la variable aquí coincida exactamente con cómo la llamaste en Windows, usualmente es OPENAI_API_KEY)
string apiKey = Environment.GetEnvironmentVariable("OPENAI_API_KEY")
    ?? throw new InvalidOperationException("La variable de entorno OPENAI_API_KEY no se encontró en Windows.");


// 1. Registro del ChatClient (Para generar texto/JSON)
IChatClient miClienteIa = new ChatClient("gpt-4o-mini", apiKey).AsIChatClient();
builder.Services.AddSingleton<IChatClient>(miClienteIa);

// --- NUEVO: Registro del Generador de Embeddings ---
// 2. Creamos el cliente de embeddings de OpenAI apuntando al modelo específico
var clienteEmbeddingsOpenAi = new EmbeddingClient("text-embedding-3-small", apiKey);

// 3. Lo convertimos a la interfaz estándar de Microsoft.Extensions.AI y lo registramos
IEmbeddingGenerator<string, Embedding<float>> miGeneradorEmbeddings = clienteEmbeddingsOpenAi.AsIEmbeddingGenerator();
builder.Services.AddSingleton(miGeneradorEmbeddings);

//// Indicar el modelo y la clave al crear el ChatClient de OpenAI.
//// 2. CREAMOS la variable
//IChatClient miClienteIa = new ChatClient("gpt-4o-mini", apiKey).AsIChatClient();

//// 3. Registrar el ChatClient de OpenAI como Singleton (solo necesitamos uno para toda la app)
//builder.Services.AddSingleton<IChatClient>(miClienteIa);
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

// 1. Registro del servicio de interpretación (El que se comunica con el LLM para extraer entidades)
// Registro del servicio de aplicación
builder.Services.AddScoped<IModuloInterpretacionService, ModuloInterpretacionService>();

// 1.1. Registro del servicio de extracción de entidades que usa el Módulo de Interpretación.
// Este es el que se comunica directamente con el LLM para extraer los parámetros de la consulta del docente.
// Su puede usar un mock o una implementación real. Aquí registramos la implementación real que usa MEAI.
builder.Services.AddScoped<IExtractorEntidades, ExtractorEntidadesMeai>();


//2. Registro del servicio orquestador (El que maneja el flujo de GraphRAG)
builder.Services.AddScoped<IRecuperadorHibridoService, RecuperadorHibridoService>();


// 3. Registro del servicio de generación de la estructura base de la evaluación (Fase 1)
builder.Services.AddScoped<IGenerarEstructuraEvaluacion, GenerarEstructuraEvaluacion>();

// 4. Registro del servicio de validación de la respuesta del LLM
builder.Services.AddScoped<IModuloValidacionRespuestaLLM, ModuloValidacionRespuestaLLM>();


// Agregar el motor de generacion
builder.Services.AddScoped<ILLMClient, MeaiLLMClient>();
builder.Services.AddScoped<IModuloGeneracionService, ModuloGeneracionService>();

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