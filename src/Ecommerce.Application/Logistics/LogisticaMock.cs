using Microsoft.Extensions.Configuration;

namespace Ecommerce.Application.Logistics;

public class LogisticaMock {

    private readonly bool _simularFalha;

    public LogisticaMock(IConfiguration configuration) {

        _simularFalha = bool.Parse(configuration["LogisticaMock:SimularFalha"] ?? "false");
    }

    public Task<string> ProcessarAsync(int pedidoId, CancellationToken cancellationToken = default) {

        cancellationToken.ThrowIfCancellationRequested();

        if (_simularFalha) {

            throw new InvalidOperationException("Falha simulada no processamento da logística.");
        }

        var codigoRastreio = $"BR{pedidoId:D8}";

        return Task.FromResult(codigoRastreio);
    }

}

