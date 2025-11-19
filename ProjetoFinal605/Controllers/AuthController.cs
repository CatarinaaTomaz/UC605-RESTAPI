using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using ProjetoFinal605.Data; // Assumindo o seu DbContext
using ProjetoFinal605.Models; // Assumindo o seu modelo Utilizador
using System.ComponentModel.DataAnnotations;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    private readonly IConfiguration _configuration;

    public AuthController(AppDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// POST api/auth/login - Autentica o utilizador e devolve um JWT.
    /// </summary>
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        // 1. Validar Credenciais (Simples, deve usar hashing na produção!)
        var utilizador = await _context.Utilizadores
            .SingleOrDefaultAsync(u => u.Email == request.Email && u.Password == request.Password);

        if (utilizador == null)
        {
            return Unauthorized(new { Message = "Credenciais inválidas." });
        }

        // 2. Criar o JWT
        var token = GenerateJwtToken(utilizador);

        return Ok(new { Token = token, utilizador.Email, utilizador.Role });
    }

    // --- Método Auxiliar para Geração de Token ---
    private string GenerateJwtToken(Utilizador utilizador)
    {
        // 2.1 Definir Claims (Identidade)
        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, utilizador.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, utilizador.Email),
            new Claim(ClaimTypes.Role, utilizador.Role) // Usado para [Authorize(Roles = "Admin")]
        };

        // 2.2 Obter Key Secreta
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]));

        // 2.3 Criar Token
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(claims),
            Expires = DateTime.UtcNow.AddHours(2), // Expira em 2 horas
            SigningCredentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature),
            Issuer = _configuration["Jwt:Issuer"],
            Audience = _configuration["Jwt:Audience"]
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var securityToken = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(securityToken);
    }
}
// --- DTO para o Body da Request ---
public class LoginRequest
{
    public string Email { get; set; }
    public string Password { get; set; }
}


// ... imports e construtor (como antes) ...   DEBUG!!!!!!!!!!!!!!

[Route("api/[controller]")]
[ApiController]
public class AuthController : ControllerBase
{
    private readonly AppDbContext _context;
    // ... outros campos ...

    // ... construtor ...

    /// <summary>
    /// POST api/auth/register - Cria um novo utilizador.
    /// </summary>
    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        // 1. Validar se o utilizador já existe
        var exists = await _context.Utilizadores.AnyAsync(u => u.Email == request.Email);
        if (exists)
        {
            return BadRequest(new { Message = "O email já está registado." });
        }

        // 2. Criar a nova entidade Utilizador
        var novoUtilizador = new Utilizador
        {
            Nome = request.Nome,
            Email = request.Email,
            Password = request.Password, // Lembre-se: Hashing é essencial em produção!
            Role = "Cliente" // Define o Role padrão
        };

        // 3. Adicionar à Base de Dados e guardar
        _context.Utilizadores.Add(novoUtilizador);
        await _context.SaveChangesAsync();

        // 4. Resposta de sucesso
        return StatusCode(201, new { Message = "Registo efetuado com sucesso. Pode agora iniciar sessão." });
    }

    // ... código do Login e GenerateJwtToken ...
}

// --- DTO para o Body da Request de Registo ---
public class RegisterRequest
{
    [Required]
    public string Nome { get; set; }

    [Required]
    public string Email { get; set; }

    [Required]
    public string Password { get; set; }
}