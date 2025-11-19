using ProjetoFinal605.Data; // Necessário para aceder aos modelos
using Microsoft.EntityFrameworkCore;
using ProjetoFinal605.Models;

// O nome da sua API e do contexto
namespace ProjetoFinal605.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        // As propriedades DbSet são as que o EF Core mapeia para tabelas na BD
        public DbSet<Utilizador> Utilizadores { get; set; }
        public DbSet<Produto> Produtos { get; set; }

        // Opcional: Configurações adicionais
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Exemplo: Garantir que o email é único (Índice único)
            modelBuilder.Entity<Utilizador>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Exemplo: Popular a BD com dados iniciais (Seed Data)
            modelBuilder.Entity<Utilizador>().HasData(
                new Utilizador { Id = 1, Nome = "Admin", Email = "admin@loja.pt", Password = "123", Role = "Admin" }
            );

            base.OnModelCreating(modelBuilder);
        }
    }
}
