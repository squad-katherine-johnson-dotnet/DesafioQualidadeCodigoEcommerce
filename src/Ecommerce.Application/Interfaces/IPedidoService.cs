using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IPedidoService {
    IEnumerable<PedidoResponseDto> ListarPedidos();
    PedidoResponseDto? ConsultarPedidoPorId(int id);
    Task<PedidoResponseDto> CriarPedidoAsync(CriarPedidoDto dto);
    Task<PedidoResponseDto?> ReprocessarPagamentoAsync(int pedidoId);
}