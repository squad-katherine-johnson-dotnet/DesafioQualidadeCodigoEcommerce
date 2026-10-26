namespace Ecommerce.Application.Payments;

public class PagamentoMock {

    public Task<bool> ProcessarAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(true);
    }
}