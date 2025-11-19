using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

[Route("api/[controller]")]
[ApiController]
[Authorize] // Exige que o utilizador esteja autenticado
public class StockController : ControllerBase
{
    // HttpClient Factory para usar Polly
    private readonly HttpClient _httpClient;
    private readonly IConfiguration _configuration;

    // O IHttpClientFactory injetado aqui é onde as políticas Polly são aplicadas
    public StockController(IHttpClientFactory httpClientFactory, IConfiguration configuration)
    {
        // Assumindo que você registrou um HttpClient nomeado 'ImposterClient' no Program.cs com Polly
        _httpClient = httpClientFactory.CreateClient("ImposterClient");
        _configuration = configuration;
    }

    /// <summary>
    /// GET api/stock/{id} - Consulta o stock (via Imposter) com Resiliência Polly.
    /// </summary>
    [HttpGet("{id}")]
    public async Task<IActionResult> GetStock(int id)
    {
        // Obtém o URL base do Imposter do appsettings.json
        var imposterBaseUrl = _configuration["Imposter:Url"];
        if (string.IsNullOrEmpty(imposterBaseUrl))
        {
            // Fallback simples se a config falhar. Pode ser mais sofisticado com Polly
            return StatusCode(503, "Serviço de inventário indisponível (URL não configurado).");
        }

        // Endpoint simulado no Mountebank/Imposter
        var requestUrl = $"{imposterBaseUrl}/inventario/{id}";

        try
        {
            // O Polly faz o retry se falhar, e o Circuit Breaker protege
            var response = await _httpClient.GetAsync(requestUrl);

            // A resposta é processada APÓS o Polly ter atuado
            if (response.IsSuccessStatusCode)
            {
                var stockData = await response.Content.ReadAsStringAsync();
                return Ok(new { Source = "Imposter (Resiliência OK)", Data = stockData });
            }
            else
            {
                // Resposta de erro do Imposter (ex: 404, 500)
                return StatusCode((int)response.StatusCode, $"Erro do Imposter: {response.ReasonPhrase}");
            }
        }
        catch (HttpRequestException ex)
        {
            // Captura erros de rede ou Circuit Breaker (Polly Fallback)
            return StatusCode(503, $"Serviço de Inventário Temporariamente Indisponível. Causa: {ex.Message}");
        }
    }
}