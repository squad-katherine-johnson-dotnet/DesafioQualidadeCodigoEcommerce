using Ecommerce.Application.Payments;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Tests;

public class PagamentoMockTests {

    [Fact]
    public async Task ProcessarAsync_DeveAprovarPagamentoQuandoConfiguracaoEstiverHabilitada() {

        // Configura o mock para aprovar pagamentos.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["PagamentoMock:AprovarPagamento"] = "true"
            })
            .Build();

        var pagamentoMock = new PagamentoMock(configuracao);

        var resultado = await pagamentoMock.ProcessarAsync(123);

        // Verifica se o pagamento foi aprovado.
        Assert.True(resultado);
    }

    [Fact]
    public async Task ProcessarAsync_DeveRecusarPagamentoQuandoConfiguracaoEstiverDesabilitada() {

        // Configura o mock para recusar pagamentos.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["PagamentoMock:AprovarPagamento"] = "false"
            })
            .Build();

        var pagamentoMock = new PagamentoMock(configuracao);

        var resultado = await pagamentoMock.ProcessarAsync(123);

        // Verifica se o pagamento foi recusado.
        Assert.False(resultado);
    }

    [Fact]
    public async Task ProcessarAsync_DeveAprovarPagamentoQuandoConfiguracaoNaoForInformada() {

        // Cria a configuração sem informar se o pagamento deve ser aprovado.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var pagamentoMock = new PagamentoMock(configuracao);

        var resultado = await pagamentoMock.ProcessarAsync(123);

        // Verifica se a configuração padrão permite aprovar o pagamento.
        Assert.True(resultado);
    }

    [Fact]
    public async Task ProcessarAsync_DeveLancarExcecaoQuandoTokenEstiverCancelado() {

        // Cria o mock e cancela o token antes de processar o pagamento.
        var configuracao = new ConfigurationBuilder().Build();
        var pagamentoMock = new PagamentoMock(configuracao);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Verifica se o processamento respeita o cancelamento solicitado.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await pagamentoMock.ProcessarAsync(123, cts.Token));
    }
}
