
using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Infrastructure.Repositories;

public class PedidoTransacaoRepository : IPedidoTransacaoRepository {
    private readonly AppDbContext _context;

    public PedidoTransacaoRepository(AppDbContext context) {
        _context = context;
    }

    public Pedido? AdicionarComBaixaDeEstoque(Pedido pedido, IEnumerable<(
        int ProdutoId, int Quantidade)> itens) {

        var itensRecebidos = itens.ToList();

        if (itensRecebidos.Count == 0 ||
            itensRecebidos.Any(item => item.ProdutoId <= 0 || item.Quantidade <= 0)) {
            return null;
        }

        var lista = itensRecebidos
            .GroupBy(item => item.ProdutoId)
            .Select(grupo => (
                ProdutoId: grupo.Key,
                Quantidade: grupo.Sum(item => item.Quantidade)))
            .ToList();

        using var transacao = _context.Database.BeginTransaction();

        try {

            foreach (var item in lista) {

                var linhasAlteradas = _context.Produtos
                    .Where(p =>
                        p.Id == item.ProdutoId &&
                        p.Estoque >= item.Quantidade)
                    .ExecuteUpdate(setters => setters
                        .SetProperty(
                            p => p.Estoque,
                            p => p.Estoque - item.Quantidade));

                if (linhasAlteradas == 0) {
                    transacao.Rollback();
                    return null;
                }
            }

            _context.Pedidos.Add(pedido);
            _context.SaveChanges();

            transacao.Commit();

            foreach (var item in lista) {
                var produtoRastreado = _context.ChangeTracker
                    .Entries<Produto>()
                    .FirstOrDefault(e => e.Entity.Id == item.ProdutoId);

                produtoRastreado?.Reload();
            }

            return pedido;
        }
        catch {
            transacao.Rollback();
            throw;
        }
    }
}
