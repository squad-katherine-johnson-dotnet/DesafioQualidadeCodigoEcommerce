using Ecommerce.Application.Interfaces;
using Ecommerce.Application.Logistics;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Microsoft.Extensions.Configuration;
using Moq;

namespace Ecommerce.Tests;

public class LogisticaServiceTests {
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<IFilaLogistica> _filaLogisticaMock;

    public LogisticaServiceTests() {
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _filaLogisticaMock = new Mock<IFilaLogistica>();
    }

    private static Pedido CriarPedido(
        StatusPagamento statusPagamento = StatusPagamento.Aprovado) {
        var produto = new Produto(1, "Teclado", 100m, 10);

        var pedido = new Pedido("Maria", "maria@email.com", "Cartão", new List<ItemPedido> { new ItemPedido(produto, 1) });

        if (statusPagamento == StatusPagamento.Aprovado) {
            pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);
        }

        return pedido;
    }

    private LogisticaService CriarServico(bool simularFalha = false) {

        // A configuração permite simular sucesso ou falha no processamento logístico.
        var configuracao = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?> {
                ["LogisticaMock:SimularFalha"] = simularFalha.ToString()
            })
            .Build();

        var logisticaMock = new LogisticaMock(configuracao);

        return new LogisticaService(_pedidoRepositoryMock.Object, logisticaMock, _filaLogisticaMock.Object);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoPedidoNaoExiste_DeveLancarExcecao() {

        // Um pedido inexistente não pode ter sua logística processada.
        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(99))
            .Returns((Pedido?)null);

        var service = CriarServico();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ProcessarLogisticaAsync(99));

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoPagamentoNaoFoiAprovado_DeveLancarExcecao() {

        // A logística não pode ser processada antes da aprovação do pagamento.
        var pedido = CriarPedido(StatusPagamento.Pendente);

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessarLogisticaAsync(1));

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoLogisticaNaoEstaPendente_DeveLancarExcecao() {

        // Um pedido cuja logística já foi processada não deve ser processado novamente.
        var pedido = CriarPedido();
        pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, "BR00000001");

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ProcessarLogisticaAsync(1));

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoProcessamentoTemSucesso_DeveAtualizarPedidoComRastreio() {

        // Quando o processamento funciona, o pedido deve ser marcado como enviado e receber o rastreio.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico();

        await service.ProcessarLogisticaAsync(1);

        Assert.Equal(StatusLogistica.Enviado, pedido.StatusLogistica);
        Assert.Equal("BR00000001", pedido.CodigoRastreio);

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(pedido), Times.Once);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
            It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoProcessamentoFalha_DeveMarcarLogisticaComoFalha() {

        // Uma falha no processamento deve ser registrada no pedido antes de propagar a exceção.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico(simularFalha: true);

        var excecao = await Assert.ThrowsAsync<InvalidOperationException>(
            () => service.ProcessarLogisticaAsync(1));

        Assert.Equal("Falha simulada no processamento da logística.", excecao.Message);

        Assert.Equal(StatusLogistica.Falhou, pedido.StatusLogistica);

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(pedido), Times.Once);
    }

    [Fact]
    public async Task ProcessarLogisticaAsync_QuandoTokenJaEstaCancelado_DevePropagarCancelamento() {

        // Um token cancelado deve impedir o processamento antes de consultar o pedido.
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var service = CriarServico();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ProcessarLogisticaAsync(1, cancellationTokenSource.Token));

        _pedidoRepositoryMock.Verify(r => r.ObterPedidoPorId(It.IsAny<int>()), Times.Never);

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarLogisticaAsync_QuandoPedidoNaoExiste_DeveLancarExcecao() {

        // Um pedido inexistente não pode ser reenviado para a fila logística.
        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(99))
            .Returns((Pedido?)null);

        var service = CriarServico();

        await Assert.ThrowsAsync<KeyNotFoundException>(() => service.ReenviarLogisticaAsync(99));

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarLogisticaAsync_QuandoPagamentoNaoFoiAprovado_DeveLancarExcecao() {

        // O reenvio logístico também depende da aprovação do pagamento.
        var pedido = CriarPedido(StatusPagamento.Pendente);
        pedido.AtualizarStatusLogistica(StatusLogistica.Falhou);

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReenviarLogisticaAsync(1));

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarLogisticaAsync_QuandoLogisticaNaoFalhou_DeveLancarExcecao() {

        // Só pedidos cuja logística falhou podem ser reenviados.
        var pedido = CriarPedido();

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        var service = CriarServico();

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.ReenviarLogisticaAsync(1));

        _pedidoRepositoryMock.Verify(
            r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task ReenviarLogisticaAsync_QuandoPedidoPodeSerReenviado_DeveRetornarParaPendenteEEnfileirar() {

        // Um pedido com pagamento aprovado e logística com falha deve voltar para pendente e entrar na fila.
        var pedido = CriarPedido();
        pedido.AtualizarStatusLogistica(StatusLogistica.Falhou);

        _pedidoRepositoryMock
            .Setup(r => r.ObterPedidoPorId(1))
            .Returns(pedido);

        _filaLogisticaMock
            .Setup(f => f.EnfileirarAsync(
                1, It.IsAny<CancellationToken>()))
            .Returns(ValueTask.CompletedTask);

        var service = CriarServico();

        await service.ReenviarLogisticaAsync(1);

        Assert.Equal(StatusLogistica.Pendente, pedido.StatusLogistica);

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(pedido), Times.Once);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
                1, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task ReenviarLogisticaAsync_QuandoTokenJaEstaCancelado_DevePropagarCancelamento() {

        // Um token cancelado deve impedir o reenvio antes de consultar o pedido.
        using var cancellationTokenSource = new CancellationTokenSource();
        cancellationTokenSource.Cancel();

        var service = CriarServico();

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.ReenviarLogisticaAsync(
                1, cancellationTokenSource.Token));

        _pedidoRepositoryMock.Verify(r => r.ObterPedidoPorId(It.IsAny<int>()), Times.Never);

        _filaLogisticaMock.Verify(f => f.EnfileirarAsync(
                It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
