using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Payments;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Ecommerce.Tests;

public class PagamentoServiceTests {
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<IFilaLogistica> _filaLogisticaMock;

    public PagamentoServiceTests() {
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _filaLogisticaMock = new Mock<IFilaLogistica>();
    }

    private PagamentoService CriarServico(bool aprovarPagamento) {

        // A configuração permite controlar se o pagamento será aprovado ou recusado.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["PagamentoMock:AprovarPagamento"] = aprovarPagamento.ToString()
            })
            .Build();

        var pagamentoMock = new PagamentoMock(configuracao);

        return new PagamentoService(_pedidoRepositoryMock.Object, pagamentoMock, _filaLogisticaMock.Object);
    }

    private static Pedido CriarPedido() {
        var produto = new Produto(1, "Teclado", 100m, 10);

        return new Pedido("Maria", "maria@email.com", "Cartão", 
            new List<ItemPedido> { new ItemPedido(produto, 1) });
    }

    [Fact]
    public async Task ProcessarPagamentoAsync_QuandoPedidoNaoExiste_DeveLancarExcecao() {

        // Um pedido inexistente não pode ter seu pagamento processado.
        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(99))
            .Returns((Pedido?)null);

        var service = CriarServico(aprovarPagamento: true);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ProcessarPagamentoAsync(99));

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(
            f => f.EnfileirarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarPagamentoAsync_QuandoPagamentoJaFoiProcessado_DeveLancarExcecao() {

        // Um pedido cujo pagamento não está pendente não deve ser processado novamente.
        var pedido = CriarPedido();
        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico(aprovarPagamento: true);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessarPagamentoAsync(1));

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(
            f => f.EnfileirarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarPagamentoAsync_QuandoPagamentoAprovado_DeveAtualizarPedidoEEnfileirarLogistica() {

        // Quando o pagamento é aprovado, o pedido deve ser atualizado e encaminhado para a logística.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        _filaLogisticaMock
            .Setup(f => f.EnfileirarAsync(0, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var service = CriarServico(aprovarPagamento: true);

        await service.ProcessarPagamentoAsync(1);

        Assert.Equal(StatusPagamento.Aprovado, pedido.StatusPagamento);

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(pedido), Times.Once);

        _filaLogisticaMock.Verify(
            f => f.EnfileirarAsync(0, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ProcessarPagamentoAsync_QuandoPagamentoRecusado_DeveAtualizarPedidoSemEnfileirarLogistica() {

        // Quando o pagamento é recusado, o status deve ser atualizado, mas a logística não deve ser iniciada.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico(aprovarPagamento: false);

        await service.ProcessarPagamentoAsync(1);

        Assert.Equal(StatusPagamento.Recusado, pedido.StatusPagamento);

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(pedido), Times.Once);

        _filaLogisticaMock.Verify(
            f => f.EnfileirarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarPagamentoAsync_QuandoTokenJaEstaCancelado_DevePropagarCancelamento() {

        // Um token já cancelado deve impedir o processamento antes de atualizar o pedido.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var service = CriarServico(aprovarPagamento: true);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ProcessarPagamentoAsync(1, cancellationTokenSource.Token));

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(
            f => f.EnfileirarAsync(It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
