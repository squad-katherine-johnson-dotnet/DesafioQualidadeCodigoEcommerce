using Ecommerce.Application.Interfaces;

namespace Ecommerce.API.BackgroundServices;

public class ProcessarPagamentoWorker : BackgroundService {

    private readonly IFilaPagamento _filaPagamento;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProcessarPagamentoWorker> _logger;

    public ProcessarPagamentoWorker(
        IFilaPagamento filaPagamento,
        IServiceScopeFactory scopeFactory,
        ILogger<ProcessarPagamentoWorker> logger) {

        _filaPagamento = filaPagamento;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        _logger.LogInformation("Worker de pagamentos iniciado.");

        while (!stoppingToken.IsCancellationRequested) {

            try {

                var pedidoId = await _filaPagamento.DesenfileirarAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();

                _logger.LogInformation("Pedido {PedidoId} retirado da fila para processamento.", pedidoId);

                var pagamentoService = scope.ServiceProvider.GetRequiredService<IPagamentoService>();

                await pagamentoService.ProcessarPagamentoAsync(pedidoId, stoppingToken);

                _logger.LogInformation("Processamento do pagamento do pedido {PedidoId} concluído.", pedidoId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                break;
            }
            catch (Exception ex) {

                _logger.LogError(ex, "Erro ao processar um pagamento ou consumir a fila.");
            }
        }

        _logger.LogInformation("Worker de pagamentos encerrado.");
    }
}

