using Ecommerce.Application.DTOs;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.Interfaces;

public interface IPedidoService {
    IEnumerable<PedidoResponseDto> ListarPedidos();
    PedidoResponseDto? ConsultarPedidoPorId(int id);
    Task<PedidoResponseDto> CriarPedidoAsync(CriarPedidoDto dto);
    PedidoResponseDto? AtualizarStatusPagamento(int pedidoId, StatusPagamento status);
    PedidoResponseDto? AtualizarStatusLogistica(int pedidoId, StatusLogistica status, string? codigoRastreio = null);
}