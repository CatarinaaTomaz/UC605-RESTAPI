using Polly;
using Polly.Extensions.Http;
using ProjetoFinal605.Data;
using Microsoft.EntityFrameworkCore;
using ProjetoFinal605.Data;

// ... No Program.cs, antes de builder.Build() ...


var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring OpenAPI at https://aka.ms/aspnet/openapi
builder.Services.AddOpenApi();

var app = builder.Build();

// 1. Política de Retry: Tenta 3 vezes, esperando 1, 2 e 4 segundos.
var retryPolicy = HttpPolicyExtensions
    .HandleTransientHttpError() // Erros 5xx ou 408 Request Timeout
    .WaitAndRetryAsync(3, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));

// 2. Política de Circuit Breaker: Se falhar 5 vezes, bloqueia por 30 segundos.
var circuitBreakerPolicy = HttpPolicyExtensions
    .HandleTransientHttpError()
    .CircuitBreakerAsync(5, TimeSpan.FromSeconds(30));

// Registar o HttpClient com as políticas combinadas
builder.Services.AddHttpClient("ImposterClient", client =>
{
    // Configurações base
    client.Timeout = TimeSpan.FromSeconds(5);
})
.AddPolicyHandler(retryPolicy) // Aplica a política de Retry
.AddPolicyHandler(circuitBreakerPolicy); // Aplica a política de Circuit Breaker

// ... continue com o código de JWT, Redis, etc.


// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();



// ... (início do Program.cs)

// Obter a String de Conexão definida no appsettings.json
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

// Registo do DbContext com o provedor (exemplo para PostgreSQL):
builder.Services.AddDbContext<AppDbContext>(options =>
{
    // Usar o provedor Npgsql para PostgreSQL
    options.UseNpgsql(connectionString);

    // Se estivesse a usar SQL Server: options.UseSqlServer(connectionString);
    // Se estivesse a usar MariaDB: options.UseMySql(connectionString, ServerVersion.AutoDetect(connectionString));
});

// ... (resto do Program.cs)


app.Run();
