namespace Ecommerce.Application.DTOs;

public class ItemPedidoResponseDto {
    public int ProdutoId { get; set; }
    public int Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}

