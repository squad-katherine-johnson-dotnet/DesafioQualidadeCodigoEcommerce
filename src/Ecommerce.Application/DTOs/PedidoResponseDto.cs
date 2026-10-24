namespace Ecommerce.Application.DTOs;

public class PedidoResponseDto
{
    public int Id { get; set; }
    public string ClienteNome { get; set; } = string.Empty;
    public string ClienteEmail { get; set; } = string.Empty;
    public DateTime DataPedido { get; set; }
    public string FormaPagamento { get; set; } = string.Empty;
    public decimal ValorBruto { get; set; }
    public decimal ValorDesconto { get; set; }
    public decimal ValorTotal { get; set; }
    public List<ItemPedidoResponseDto> Itens { get; set; } = [];
}

public class ItemPedidoResponseDto
{
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}