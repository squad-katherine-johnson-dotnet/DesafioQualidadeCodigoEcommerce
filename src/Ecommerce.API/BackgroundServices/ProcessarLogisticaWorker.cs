using Ecommerce.Application.Interfaces;

namespace Ecommerce.API.BackgroundServices;

public class ProcessarLogisticaWorker : BackgroundService {

    private readonly IFilaLogistica _filaLogistica;
    private readonly IServiceScopeFactory _scopeFactory;
    private readonly ILogger<ProcessarLogisticaWorker> _logger;

    public ProcessarLogisticaWorker(
        IFilaLogistica filaLogistica,
        IServiceScopeFactory scopeFactory,
        ILogger<ProcessarLogisticaWorker> logger) {

        _filaLogistica = filaLogistica;
        _scopeFactory = scopeFactory;
        _logger = logger;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken) {

        _logger.LogInformation("Worker de logística iniciado.");

        while (!stoppingToken.IsCancellationRequested) {

            try {

                var pedidoId = await _filaLogistica.DesenfileirarAsync(stoppingToken);

                using var scope = _scopeFactory.CreateScope();

                _logger.LogInformation("Pedido {PedidoId} retirado da fila de logística.", pedidoId);

                var logisticaService = scope.ServiceProvider.GetRequiredService<ILogisticaService>();

                await logisticaService.ProcessarLogisticaAsync(pedidoId, stoppingToken);

                _logger.LogInformation("Processamento da logística do pedido {PedidoId} concluído", pedidoId);
            }
            catch (OperationCanceledException) when (stoppingToken.IsCancellationRequested) {
                break;
            }
            catch (Exception ex) {

                _logger.LogError(ex, "Erro ao processar a logística ou consumir a fila.");
            }
        }

        _logger.LogInformation("Worker da logística encerrado.");
    }
}
