using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Logistics;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class LogisticaService : ILogisticaService {

    private readonly IPedidoRepository _pedidoRepository;
    private readonly LogisticaMock _logisticaMock;
    private readonly IFilaLogistica _filaLogistica;


    public LogisticaService(
        IPedidoRepository pedidoRepository,
        LogisticaMock logisticaMock,
        IFilaLogistica filaLogistica) {

        _pedidoRepository = pedidoRepository;
        _logisticaMock = logisticaMock;
        _filaLogistica = filaLogistica;
    }

    public async Task ProcessarLogisticaAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) throw new KeyNotFoundException($"Pedido com ID {pedidoId} não foi encontrado.");

        if (pedido.StatusPagamento != StatusPagamento.Aprovado) throw new InvalidOperationException(
            "A logística só pode ser processada após a aprovação do pagamento.");

        if (pedido.StatusLogistica != StatusLogistica.Pendente) throw new InvalidOperationException(
            "Somente pedidos com logística pendente podem ser processados.");

        string codigoRastreio;

        try {
            codigoRastreio = await _logisticaMock.ProcessarAsync(pedidoId, cancellationToken);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            throw;
        }
        catch (Exception) {

            pedido.AtualizarStatusLogistica(StatusLogistica.Falhou);

            _pedidoRepository.AtualizarPedido(pedido);
            throw;
        }

        cancellationToken.ThrowIfCancellationRequested();

        pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, codigoRastreio);

        _pedidoRepository.AtualizarPedido(pedido);
    }

    public async Task ReenviarLogisticaAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) throw new KeyNotFoundException(
                $"Pedido com ID {pedidoId} não foi encontrado.");

        if (pedido.StatusPagamento != StatusPagamento.Aprovado) throw new InvalidOperationException(
                "A logística só pode ser reenviada após a aprovação do pagamento.");

        if (pedido.StatusLogistica != StatusLogistica.Falhou) throw new InvalidOperationException(
                "Somente pedidos com logística com falha podem ser reenviados.");

        pedido.AtualizarStatusLogistica(StatusLogistica.Pendente);

        _pedidoRepository.AtualizarPedido(pedido);

        await _filaLogistica.EnfileirarAsync(pedidoId, cancellationToken);
    }
}

