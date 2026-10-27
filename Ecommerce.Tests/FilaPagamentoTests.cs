using Ecommerce.Application.Queues;

namespace Ecommerce.Tests;

public class FilaPagamentoTests {

    [Fact]
    public async Task EnfileirarAsync_DevePermitirDesenfileirarMesmoPedido() {

        // Cria a fila e define o ID do pedido.
        var fila = new FilaPagamento();
        var pedidoId = 123;

        // Enfileira o pedido e depois o retira da fila.
        await fila.EnfileirarAsync(pedidoId);
        var resultado = await fila.DesenfileirarAsync();

        // Assert: verifica se o pedido retirado é o mesmo que foi enfileirado.
        Assert.Equal(pedidoId, resultado);
    }

    [Fact]
    public async Task EnfileirarAsync_DevePreservarOrdemDosPedidos() {

        // Cria a fila com três pedidos.
        var fila = new FilaPagamento();

        // Enfileira os pedidos em sequência.
        await fila.EnfileirarAsync(1);
        await fila.EnfileirarAsync(2);
        await fila.EnfileirarAsync(3);

        var primeiro = await fila.DesenfileirarAsync();
        var segundo = await fila.DesenfileirarAsync();
        var terceiro = await fila.DesenfileirarAsync();

        // Verifica se os pedidos são retirados na mesma ordem em que entraram.
        Assert.Equal(1, primeiro);
        Assert.Equal(2, segundo);
        Assert.Equal(3, terceiro);
    }

    [Fact]
    public async Task EnfileirarAsync_DeveLancarExcecaoQuandoTokenEstiverCancelado() {

        // Cria a fila e um token já cancelado.
        var fila = new FilaPagamento();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Verifica se o cancelamento impede o enfileiramento.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await fila.EnfileirarAsync(123, cts.Token));
    }

    [Fact]
    public async Task DesenfileirarAsync_DeveLancarExcecaoQuandoTokenEstiverCancelado() {

        // Cria a fila vazia e um token já cancelado.
        var fila = new FilaPagamento();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        // Verifica se a leitura é cancelada sem um pedido disponível.
        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            async () => await fila.DesenfileirarAsync(cts.Token));
    }
}