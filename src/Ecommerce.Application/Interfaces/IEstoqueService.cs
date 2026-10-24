using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IEstoqueService {
    IEnumerable<ProdutoResponseDto> ConsultarEstoque();
    ProdutoResponseDto? ConsultarEstoquePorProdutoId(int produtoId);
    ProdutoResponseDto? RegistrarEntradaDeEstoque(int produtoId, int quantidade);
    ProdutoResponseDto? RegistrarSaidaDeEstoque(int produtoId, int quantidade);
    IEnumerable<ProdutoResponseDto> ListarProdutosAbaixoDoEstoqueMinimo(int quantidadeMinima);
    bool RegistrarSaidasDeEstoqueEmLote(IEnumerable<(int ProdutoId, int Quantidade)> itens);
}