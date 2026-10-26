namespace Ecommerce.Application.Logistics;

public class LogisticaMock {

    public Task<string> ProcessarAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        var codigoRastreio = $"BR{pedidoId:D8}";

        return Task.FromResult(codigoRastreio);
    }

}

