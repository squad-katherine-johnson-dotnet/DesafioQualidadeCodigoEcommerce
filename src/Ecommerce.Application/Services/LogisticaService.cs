using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Logistics;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class LogisticaService : ILogisticaService {

    private readonly IPedidoRepository _pedidoRepository;
    private readonly LogisticaMock _logisticaMock;

    public LogisticaService(
        IPedidoRepository pedidoRepository,
        LogisticaMock logisticaMock) {

        _pedidoRepository = pedidoRepository;
        _logisticaMock = logisticaMock;
    }

    public async Task ProcessarLogisticaAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) throw new KeyNotFoundException($"Pedido com ID {pedidoId} não foi encontrado.");

        if (pedido.StatusPagamento != StatusPagamento.Aprovado) throw new InvalidOperationException(
            "A logística só pode ser processada após a aprovação do pagamento.");

        if (pedido.StatusLogistica != StatusLogistica.Pendente) throw new InvalidOperationException(
            "Somente pedidos com logística pendente podem ser processados.");

        var codigoRastreio = await _logisticaMock.ProcessarAsync(pedidoId, cancellationToken);

        cancellationToken.ThrowIfCancellationRequested();

        pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, codigoRastreio);

        _pedidoRepository.AtualizarPedido(pedido);
    }
}

