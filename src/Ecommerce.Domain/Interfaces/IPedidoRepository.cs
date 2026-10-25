using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IPedidoRepository
{
    Pedido? ObterPedidoPorId(int id);
    IEnumerable<Pedido> ListarPedidos();
}