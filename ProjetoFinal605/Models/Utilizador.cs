using System.ComponentModel.DataAnnotations;

public class Utilizador
{
    // Chave Primária
    public int Id { get; set; }

    // Campos de Login
    [Required]
    public string Nome { get; set; }

    [Required]
    [EmailAddress]
    public string Email { get; set; }

    // Nota: Em produção, a password deve ser HASHED (ex: Argon2 ou Bcrypt).
    [Required]
    public string Password { get; set; }

    // Campo de Autorização (Para [Authorize(Roles = "Admin")])
    // Pode ser "Admin", "Cliente", "Staff"
    [Required]
    public string Role { get; set; } = "Cliente";
}