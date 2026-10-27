using Ecommerce.Application.Logistics;
using Microsoft.Extensions.Configuration;

namespace Ecommerce.Tests;

public class LogisticaMockTests {

    [Fact]
    public async Task ProcessarAsync_DeveRetornarCodigoRastreioQuandoNaoHouverFalha() {

        // Configura o mock sem falha simulada e define o ID do pedido.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["LogisticaMock:SimularFalha"] = "false"
            })
            .Build();

        var logisticaMock = new LogisticaMock(configuracao);

        var resultado = await logisticaMock.ProcessarAsync(123);

        // Verifica se o código de rastreio segue o formato esperado.
        Assert.Equal("BR00000123", resultado);
    }

    [Fact]
    public async Task ProcessarAsync_DeveLancarExcecaoQuandoFalhaEstiverHabilitada() {

        // Configura o mock para simular uma falha no processamento da logística.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["LogisticaMock:SimularFalha"] = "true"
            })
            .Build();

        var logisticaMock = new LogisticaMock(configuracao);

        // Verifica se o processamento lança a exceção esperada.
        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            async () => await logisticaMock.ProcessarAsync(123));

        Assert.Equal(
            "Falha simulada no processamento da logística.",
            excecao.Message);
    }

    [Fact]
    public async Task ProcessarAsync_DeveRetornarCodigoRastreioQuandoConfiguracaoNaoForInformada() {

        // Cria a configuração sem informar se a logística deve falhar.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>())
            .Build();

        var logisticaMock = new LogisticaMock(configuracao);

        var resultado = await logisticaMock.ProcessarAsync(123);

        // Verifica se a configuração padrão permite processar a logística.
        Assert.Equal("BR00000123", resultado);
    }

    [Fact]
    public async Task ProcessarAsync_DeveLancarExcecaoQuandoTokenEstiverCancelado() {

        // Cria o mock e cancela o token antes de processar a logística.
        var configuracao = new ConfigurationBuilder().Build();
        var logisticaMock = new LogisticaMock(configuracao);

        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Verifica se o processamento respeita o cancelamento solicitado.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await logisticaMock.ProcessarAsync(123, cts.Token));
    }
}
