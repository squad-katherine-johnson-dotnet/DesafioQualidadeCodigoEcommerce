using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public class PedidoRepository : IPedidoRepository
{
    private readonly AppDbContext _context;

    public PedidoRepository(AppDbContext context)
    {
        _context = context;
    }

    public Pedido Adicionar(Pedido pedido)
    {
        _context.Pedidos.Add(pedido);
        _context.SaveChanges();

        return pedido;
    }

    public Pedido? ObterPorId(int id)
    {
        return _context.Pedidos
            .Include(pedido => pedido.Itens)
            .FirstOrDefault(pedido => pedido.Id == id);
    }

    public IEnumerable<Pedido> ObterTodos()
    {
        return _context.Pedidos
            .Include(pedido => pedido.Itens)
            .AsNoTracking()
            .ToList();
    }
}