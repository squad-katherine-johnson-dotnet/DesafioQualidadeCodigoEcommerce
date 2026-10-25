using Ecommerce.Application.DTOs;

namespace Ecommerce.Application.Interfaces;

public interface IPedidoService {
    IEnumerable<PedidoResponseDto> ListarPedidos();
    PedidoResponseDto? ConsultarPedidoPorId(int id);
    PedidoResponseDto CriarPedido(CriarPedidoDto dto);
}