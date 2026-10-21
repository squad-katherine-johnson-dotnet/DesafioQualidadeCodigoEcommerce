namespace Ecommerce.Application.DTOs;

public class CriarProdutoDto
{
    public string Nome { get; set; } = string.Empty;
    public decimal Preco { get; set; }
    public int Estoque { get; set; }
}
