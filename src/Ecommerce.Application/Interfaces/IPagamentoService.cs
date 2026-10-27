namespace Ecommerce.Application.Interfaces;

public interface IPagamentoService {
    Task ProcessarPagamentoAsync(int pedidoId, CancellationToken cancellationToken = default);
}

