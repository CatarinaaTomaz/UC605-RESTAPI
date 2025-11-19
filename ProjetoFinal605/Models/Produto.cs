using System.ComponentModel.DataAnnotations;

public class Produto
{
    // Chave Primária
    public int Id { get; set; }

    [Required]
    [MaxLength(100)]
    public string Nome { get; set; }

    [MaxLength(500)]
    public string Descricao { get; set; }

    [Required]
    public decimal Preco { get; set; } // O tipo decimal é crucial para dinheiro

    [Required]
    public string Categoria { get; set; }

    // O Stock real é consultado no Imposter, mas podemos ter um campo local
    // para um valor default ou cache de última leitura, se necessário.
    // Ou pode ser um campo virtual que chama a API de Stock.
    // Por simplicidade, vamos mantê-lo limpo e usar o StockController para a consulta.

    // Campo de Gestão
    public bool Ativo { get; set; } = true; // Para "soft delete" (não apagar da BD)

    // Propriedade para simular a ligação ao serviço externo (opcional)
    public string CodigoInventarioExterno { get; set; }
}