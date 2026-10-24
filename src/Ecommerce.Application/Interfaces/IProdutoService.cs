using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IProdutoService {
    IEnumerable<ProdutoResponseDto> ListarProdutos();
    ProdutoResponseDto? ConsultarProdutoPorId(int id);
    ProdutoResponseDto CriarProduto(CriarProdutoDto dto);
    bool AtualizarProduto(int id, AtualizarProdutoDto dto);
    bool RemoverProduto(int id);
}