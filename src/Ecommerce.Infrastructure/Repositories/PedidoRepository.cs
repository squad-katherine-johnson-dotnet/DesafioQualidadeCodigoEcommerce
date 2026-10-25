using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository {
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context) {
        _context = context;
    }

    public Pedido? ObterPedidoPorId(int id) {

        return _context.Pedidos
            .Include(pedido => pedido.Itens)
            .FirstOrDefault(pedido => pedido.Id == id);
    }

    public IEnumerable<Pedido> ListarPedidos() {

        return _context.Pedidos
            .Include(pedido => pedido.Itens)
            .AsNoTracking()
            .ToList();
    }
}