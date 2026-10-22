using System.Collections.Concurrent;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Infrastructure.Repositories;

public class ProdutoRepository : IProdutoRepository
{
    private readonly ConcurrentDictionary<int, Produto> _produtos = new();
    private int _proximoId = 1;

    public IEnumerable<Produto> ObterTodos() => _produtos.Values;

    public Produto? ObterPorId(int id)
    {
        _produtos.TryGetValue(id, out var produto);
        return produto;
    }

    public Produto Adicionar(Produto produto)
    {
        var id = _proximoId++;
        var novoProduto = new Produto(id, produto.Nome, produto.Preco, produto.Estoque);
        _produtos[id] = novoProduto;
        return novoProduto;
    }

    public bool Atualizar(Produto produto)
    {
        if (!_produtos.ContainsKey(produto.Id)) return false;
        _produtos[produto.Id] = produto;
        return true;
    }

    public bool Remover(int id)
    {
        return _produtos.TryRemove(id, out _);
    }
}