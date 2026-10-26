namespace Ecommerce.Application.Interfaces;

public interface ILogisticaService {
    Task ProcessarLogisticaAsync(int pedidoId, CancellationToken cancellationToken = default);
    Task ReenviarLogisticaAsync(int pedidoId, CancellationToken cancellationToken = default);
}

