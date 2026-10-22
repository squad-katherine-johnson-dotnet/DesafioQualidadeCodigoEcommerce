using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class EstoqueService : IEstoqueService
{
    private readonly IProdutoRepository _repository;

    public EstoqueService(IProdutoRepository repository)
    {
        _repository = repository;
    }

    public IEnumerable<ProdutoResponseDto> ObterTodos()
    {
        return _repository.ObterTodos()
            .Select(produto => MapearProduto(produto));
    }

    public ProdutoResponseDto? ObterPorId(int produtoId)
    {
        var produto = _repository.ObterPorId(produtoId);

        if (produto == null)
            return null;

        return MapearProduto(produto);
    }

    public ProdutoResponseDto? RegistrarEntrada(
        int produtoId,
        int quantidade)
    {
        var produto = _repository.ObterPorId(produtoId);

        if (produto == null)
            return null;

        produto.RegistrarEntradaEstoque(quantidade);

        var atualizado = _repository.Atualizar(produto);

        if (!atualizado)
            return null;

        return MapearProduto(produto);
    }

    public ProdutoResponseDto? RegistrarSaida(
        int produtoId,
        int quantidade)
    {
        var produto = _repository.ObterPorId(produtoId);

        if (produto == null)
            return null;

        produto.RegistrarSaidaEstoque(quantidade);

        var atualizado = _repository.Atualizar(produto);

        if (!atualizado)
            return null;

        return MapearProduto(produto);
    }

    public IEnumerable<ProdutoResponseDto> ObterAbaixoDoMinimo(
        int quantidadeMinima)
    {
        if (quantidadeMinima < 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantidadeMinima),
                "O estoque mínimo não pode ser negativo.");

        return _repository.ObterTodos()
            .Where(produto => produto.Estoque < quantidadeMinima)
            .Select(produto => MapearProduto(produto));
    }

    private static ProdutoResponseDto MapearProduto(
        Ecommerce.Domain.Entities.Produto produto)
    {
        return new ProdutoResponseDto
        {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Estoque = produto.Estoque
        };
    }
}