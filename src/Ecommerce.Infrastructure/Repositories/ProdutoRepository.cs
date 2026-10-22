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

    public bool Atualizar(Produto produto)
    {
        var existente = _context.Produtos.Find(produto.Id);
        if (existente == null) return false;

        _context.Entry(existente).CurrentValues.SetValues(produto);
        _context.SaveChanges();
        return true;
    }

    public bool Remover(int id)
    {
        var produto = _context.Produtos.Find(id);
        if (produto == null) return false;

        _context.Produtos.Remove(produto);
        _context.SaveChanges();
        return true;
    }
}