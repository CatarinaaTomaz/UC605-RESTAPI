using ProjetoFinal605.Data;
using ProjetoFinal605.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Caching.Distributed; // Para Redis
using Microsoft.Extensions.Caching.Memory; // Para Polly Cache In-Memory
using System;
using System.Text.Json;

[Route("api/[controller]")]
[ApiController]
public class ProdutoController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IDistributedCache _redisCache; // Serviço Redis
    private readonly IMemoryCache _memoryCache;   // Serviço In-Memory (Polly Cache)

    public ProdutoController(AppDbContext context, IDistributedCache redisCache, IMemoryCache memoryCache)
    {
        _context = context;
        _redisCache = redisCache;
        _memoryCache = memoryCache;
    }

    /// <summary>
    /// GET api/produtos - Lista produtos usando Cache Híbrido (Polly -> Redis -> BD).
    /// </summary>
    [HttpGet]
    public async Task<IActionResult> GetProdutos()
    {
        const string CacheKey = "ProdutosList";
        List<Produto> produtos;

        // 1. Tentar ler do CACHE IN-MEMORY (Simulação Polly Cache)
        if (_memoryCache.TryGetValue(CacheKey, out string? memoryCachedData))
        {
            produtos = JsonSerializer.Deserialize<List<Produto>>(memoryCachedData)!;
            return Ok(new { Source = "MemoryCache (Polly)", Data = produtos });
        }

        // 2. Tentar ler do REDIS CACHE
        var redisCachedData = await _redisCache.GetStringAsync(CacheKey);
        if (!string.IsNullOrEmpty(redisCachedData))
        {
            produtos = JsonSerializer.Deserialize<List<Produto>>(redisCachedData)!;
            // Atualiza o Cache In-Memory (Polly Cache) antes de devolver
            _memoryCache.Set(CacheKey, redisCachedData, TimeSpan.FromMinutes(1));
            return Ok(new { Source = "RedisCache", Data = produtos });
        }

        // 3. Consultar a BASE DE DADOS
        produtos = await _context.Produtos.ToListAsync();

        // 4. Se houver dados, atualizar os caches
        if (produtos.Any())
        {
            var serializedData = JsonSerializer.Serialize(produtos);

            // Define opções de Cache (ex: expiração de 5 minutos no Redis)
            var options = new DistributedCacheEntryOptions().SetAbsoluteExpiration(TimeSpan.FromMinutes(5));

            await _redisCache.SetStringAsync(CacheKey, serializedData, options);
            _memoryCache.Set(CacheKey, serializedData, TimeSpan.FromMinutes(1)); // Menor expiração no local

            return Ok(new { Source = "Database", Data = produtos });
        }

        return NotFound("Nenhum produto encontrado.");
    }
}