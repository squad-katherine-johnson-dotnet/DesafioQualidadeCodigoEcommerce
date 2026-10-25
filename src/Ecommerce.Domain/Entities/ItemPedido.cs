namespace Ecommerce.Domain.Entities;

public class ItemPedido {
    public int Id { get; private set; }
    public int ProdutoId { get; private set; }
    public int Quantidade { get; private set; }
    public decimal PrecoUnitario { get; private set; }

    private ItemPedido() { }

    public ItemPedido(Produto produto, int quantidade) {

        ArgumentNullException.ThrowIfNull(produto);

        if (quantidade <= 0) throw new ArgumentOutOfRangeException(
                nameof(quantidade), "A quantidade deve ser maior que zero.");

        ProdutoId = produto.Id;
        Quantidade = quantidade;
        PrecoUnitario = produto.Preco;
    }

    public decimal CalcularSubtotal() {

        return PrecoUnitario * Quantidade;
    }
}