using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IProdutoService
{
    IEnumerable<ProdutoResponseDto> ObterTodos();
    ProdutoResponseDto? ObterPorId(int id);
    ProdutoResponseDto Criar(CriarProdutoDto dto);
    bool Atualizar(int id, AtualizarProdutoDto dto);
    bool Remover(int id);
}