using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IPedidoRepository
{
    Pedido Adicionar(Pedido pedido);
    Pedido? ObterPorId(int id);
    IEnumerable<Pedido> ObterTodos();
}