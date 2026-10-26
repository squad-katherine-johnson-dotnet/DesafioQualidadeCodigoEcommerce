using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Payments;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class PagamentoService : IPagamentoService {

    private readonly IPedidoRepository _pedidoRepository;
    private readonly PagamentoMock _pagamentoMock;
    private readonly IFilaLogistica _filaLogistica;

    public PagamentoService(
        IPedidoRepository pedidoRepository,
        PagamentoMock pagamentoMock,
        IFilaLogistica filaLogistica) {

        _pedidoRepository = pedidoRepository;
        _pagamentoMock = pagamentoMock;
        _filaLogistica = filaLogistica;
    }

    public async Task ProcessarPagamentoAsync(int pedidoId, CancellationToken cancellationToken = default) {

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) throw new KeyNotFoundException(
            $"Pedido com ID {pedidoId} não foi encontrado.");

        if (pedido.StatusPagamento != StatusPagamento.Pendente) throw new InvalidOperationException(
            "Somente pagamento pendentes podem ser processados.");

        var aprovado = await _pagamentoMock.ProcessarAsync(pedidoId, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        pedido.AtualizarStatusPagamento(aprovado ? StatusPagamento.Aprovado : StatusPagamento.Recusado);

        _pedidoRepository.AtualizarPedido(pedido);

        if (aprovado) {

            await _filaLogistica.EnfileirarAsync(pedido.Id, cancellationToken);
        }
    }
}

