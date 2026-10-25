
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class ProdutoService : IProdutoService {
    private readonly IProdutoRepository _repository;

    public ProdutoService(IProdutoRepository repository) {
        _repository = repository;
    }

    public IEnumerable<ProdutoResponseDto> ListarProdutos() {

        return _repository.ObterTodos()
            .Select(MapearProduto);
    }

    public ProdutoResponseDto? ConsultarProdutoPorId(int id) {

        var produto = _repository.ObterPorId(id);

        if (produto == null) return null;

        return MapearProduto(produto);
    }

    public ProdutoResponseDto CriarProduto(CriarProdutoDto dto) {

        var novoProduto = new Produto(0, dto.Nome, dto.Preco, dto.Estoque);

        var produtoCriado = _repository.Adicionar(novoProduto);

        return MapearProduto(produtoCriado);
    }

    public bool AtualizarProduto(int id, AtualizarProdutoDto dto) {

        var produtoExistente = _repository.ObterPorId(id);

        if (produtoExistente == null) return false;

        produtoExistente.AtualizarDados(dto.Nome, dto.Preco, dto.Estoque);

        return _repository.Atualizar(produtoExistente);
    }

    public bool RemoverProduto(int id) {

        return _repository.Remover(id);
    }

    private static ProdutoResponseDto MapearProduto(Produto produto) {

        return new ProdutoResponseDto {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Estoque = produto.Estoque
        };
    }
}
