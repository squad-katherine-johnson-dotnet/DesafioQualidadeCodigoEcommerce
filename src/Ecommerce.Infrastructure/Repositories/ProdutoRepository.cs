using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Infrastructure.Context;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly AppDbContext _context;

    public ProdutoRepository(AppDbContext context)
    {
        _context = context;
    }

    public IEnumerable<Produto> ObterTodos()
    {
        return _context.Produtos.AsNoTracking().ToList();
    }

    public Produto? ObterPorId(int id)
    {
        return _context.Produtos.Find(id);
    }

    public Produto Adicionar(Produto produto)
    {
        _context.Produtos.Add(produto);
        _context.SaveChanges();
        return produto;
    }

    public bool Atualizar(Produto produto) {
        var existente = _context.Produtos.Find(produto.Id);

        if (existente == null)
            return false;

        var valoresOriginais = _context.Entry(existente)
            .CurrentValues.Clone();

        try {
            _context.Entry(existente).CurrentValues.SetValues(produto);
            _context.SaveChanges();

            return true;
        }
        catch {
            _context.Entry(existente).CurrentValues.SetValues(valoresOriginais);
            throw;
        }
    }

    public bool Remover(int id)
    {
        var produto = _context.Produtos.Find(id);
        if (produto == null) return false;

        _context.Produtos.Remove(produto);
        _context.SaveChanges();
        return true;
    }
    public int BaixarEstoqueSeDisponivel(int produtoId, int quantidade)
    {
        if (quantidade <= 0)
            return 0;

        var linhasAlteradas = _context.Produtos
            .Where(p => p.Id == produtoId && p.Estoque >= quantidade)
            .ExecuteUpdate(setters => setters
                .SetProperty(p => p.Estoque, p => p.Estoque - quantidade));

        if (linhasAlteradas > 0)
        {
            var produtoRastreado = _context.ChangeTracker
                .Entries<Produto>()
                .FirstOrDefault(e => e.Entity.Id == produtoId);

            produtoRastreado?.Reload();
        }

        return linhasAlteradas;
    }

    public bool BaixarEstoqueEmLote(
        IEnumerable<(int ProdutoId, int Quantidade)> itens) {
        var lista = itens.ToList();

        if (lista.Count == 0)
            return true;

        if (lista.Any(item => item.ProdutoId <= 0 || item.Quantidade <= 0))
            return false;

        foreach (var item in lista) {
            var linhasAlteradas = _context.Produtos
                .Where(p =>
                    p.Id == item.ProdutoId &&
                    p.Estoque >= item.Quantidade)
                .ExecuteUpdate(setters => setters
                    .SetProperty(p => p.Estoque, p => p.Estoque - item.Quantidade));

            if (linhasAlteradas == 0)
                return false;
        }

        foreach (var item in lista) {
            var produtoRastreado = _context.ChangeTracker
                .Entries<Produto>()
                .FirstOrDefault(e => e.Entity.Id == item.ProdutoId);

            produtoRastreado?.Reload();
        }

        return true;
    }
}