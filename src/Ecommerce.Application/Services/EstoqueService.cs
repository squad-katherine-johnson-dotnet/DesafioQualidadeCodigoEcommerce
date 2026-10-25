using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class EstoqueService : IEstoqueService {
    private readonly IProdutoRepository _repository;

    public EstoqueService(IProdutoRepository repository) {
        _repository = repository;
    }

    public IEnumerable<ProdutoResponseDto> ConsultarEstoque() {

        return _repository.ObterTodos()
            .Select(produto => MapearProduto(produto));
    }

    public ProdutoResponseDto? ConsultarEstoquePorProdutoId(int produtoId) {

        var produto = _repository.ObterPorId(produtoId);

        if (produto == null) return null;

        return MapearProduto(produto);
    }

    public ProdutoResponseDto? RegistrarEntradaDeEstoque(int produtoId, int quantidade) {

        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantidade), "A quantidade deve ser maior que zero.");

        var produto = _repository.ObterPorId(produtoId);

        if (produto == null) return null;

        produto.RegistrarEntradaEstoque(quantidade);

        var atualizado = _repository.Atualizar(produto);

        if (!atualizado)
            throw new InvalidOperationException("Não foi possível atualizar o estoque do produto.");

        return MapearProduto(produto);
    }

    public ProdutoResponseDto? RegistrarSaidaDeEstoque(int produtoId, int quantidade) {

        if (quantidade <= 0) throw new ArgumentOutOfRangeException(
                nameof(quantidade), "A quantidade deve ser maior que zero.");

        var produto = _repository.ObterPorId(produtoId);

        if (produto == null) return null;

        var linhasAlteradas = _repository.BaixarEstoqueSeDisponivel(produtoId, quantidade);

        if (linhasAlteradas == 0)
            throw new InvalidOperationException("Não foi possível realizar a saída. Verifique se o estoque é suficiente.");

        var atualizado = _repository.ObterPorId(produtoId);

        if (atualizado == null)
            throw new InvalidOperationException("O estoque foi atualizado, mas não foi possível recuperar o produto.");

        return MapearProduto(atualizado);
    }

    public IEnumerable<ProdutoResponseDto> ListarProdutosAbaixoDoEstoqueMinimo(int quantidadeMinima) {

        if (quantidadeMinima < 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantidadeMinima), "O estoque mínimo não pode ser negativo.");

        return _repository.ObterTodos()
            .Where(produto => produto.Estoque < quantidadeMinima)
            .Select(produto => MapearProduto(produto));
    }

    public bool RegistrarSaidasDeEstoqueEmLote(IEnumerable<(int ProdutoId, int Quantidade)> itens) {

        return _repository.BaixarEstoqueEmLote(itens);
    }

    private static ProdutoResponseDto MapearProduto(Ecommerce.Domain.Entities.Produto produto) {

        return new ProdutoResponseDto {
            Id = produto.Id,
            Nome = produto.Nome,
            Preco = produto.Preco,
            Estoque = produto.Estoque
        };
    }
}