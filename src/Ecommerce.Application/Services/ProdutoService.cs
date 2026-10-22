using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class ProdutoService : IProdutoService
{
    private readonly IProdutoRepository _repository;

    public ProdutoService(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ProdutoResponseDto> ObterTodos()
    {
        return _repository.ObterTodos().Select(produto => new ProdutoResponseDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Estoque = produto.Estoque
        });
    }

    public ProdutoResponseDto? ObterPorId(int id)
    {
        var produto = _repository.ObterPorId(id);
        if (produto == null) return null;

        return new ProdutoResponseDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Estoque = produto.Estoque
        };
    }

    public ProdutoResponseDto Criar(CriarProdutoDto dto)
    {
        var novoProduto = new Produto(0, dto.Nome, dto.Preco, dto.Estoque);
        var produtoCriado = _repository.Adicionar(novoProduto);

        return new ProdutoResponseDto
        {
            Id = produtoCriado.Id,
            Nome = produtoCriado.Nome,
            Preco = produtoCriado.Preco,
            Estoque = produtoCriado.Estoque
        };
    }

    public bool Atualizar(int id, AtualizarProdutoDto dto)
    {
        var produtoExistente = _repository.ObterPorId(id);
        if (produtoExistente == null) return false;

        produtoExistente.AtualizarDados(dto.Nome, dto.Preco, dto.Estoque);
        return _repository.Atualizar(produtoExistente);
    }

    public bool Remover(int id)
    {
        return _repository.Remover(id);
    }
}