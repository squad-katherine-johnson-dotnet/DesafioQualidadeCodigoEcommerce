using Microsoft.Extensions.Configuration;

namespace Ecommerce.Application.Payments;

public class PagamentoMock {
    private readonly bool _aprovarPagamento;

    public PagamentoMock(IConfiguration configuration) {

        _aprovarPagamento = bool.Parse(configuration["PagamentoMock:AprovarPagamento"] ?? "true");
    }

    public Task<bool> ProcessarAsync(
        int pedidoId,
        CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        return Task.FromResult(_aprovarPagamento);
    }
}
