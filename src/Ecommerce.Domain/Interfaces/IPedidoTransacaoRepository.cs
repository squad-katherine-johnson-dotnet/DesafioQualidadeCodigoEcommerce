using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IPedidoTransacaoRepository {
    Pedido? AdicionarComBaixaDeEstoque(Pedido pedido, IEnumerable<(int ProdutoId, int Quantidade)> itens);
}