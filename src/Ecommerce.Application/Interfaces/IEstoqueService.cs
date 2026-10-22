using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IEstoqueService
{
    IEnumerable<ProdutoResponseDto> ObterTodos();
    ProdutoResponseDto? ObterPorId(int produtoId);
    ProdutoResponseDto? RegistrarEntrada(int produtoId, int quantidade);
    ProdutoResponseDto? RegistrarSaida(int produtoId, int quantidade);
    IEnumerable<ProdutoResponseDto> ObterAbaixoDoMinimo(int quantidadeMinima);
}