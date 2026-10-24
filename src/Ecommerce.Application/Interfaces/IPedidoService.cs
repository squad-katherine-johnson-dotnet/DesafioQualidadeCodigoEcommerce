using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IPedidoService
{
    IEnumerable<PedidoResponseDto> ObterTodos();
    PedidoResponseDto? ObterPorId(int id);
    PedidoResponseDto Criar(CriarPedidoDto dto);
}