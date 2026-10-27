using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;
using Ecommerce.Application.Interfaces;
using Moq;

namespace Ecommerce.Tests;

public class PedidoServiceTests {
    private readonly Mock<IPedidoRepository> _pedidoRepositoryMock;
    private readonly Mock<IProdutoRepository> _produtoRepositoryMock;
    private readonly Mock<IPedidoTransacaoRepository> _pedidoTransacaoRepositoryMock;
    private readonly Mock<IFilaPagamento> _filaPagamentoMock;
    private readonly PedidoService _service;

    public PedidoServiceTests() {
        _pedidoRepositoryMock = new Mock<IPedidoRepository>();
        _produtoRepositoryMock = new Mock<IProdutoRepository>();
        _pedidoTransacaoRepositoryMock = new Mock<IPedidoTransacaoRepository>();
        _filaPagamentoMock = new Mock<IFilaPagamento>();

        _service = new PedidoService(
            _pedidoRepositoryMock.Object,
            _produtoRepositoryMock.Object,
            _pedidoTransacaoRepositoryMock.Object,
            _filaPagamentoMock.Object);
    }

    private static Pedido CriarPedidoComStatus(StatusPagamento status) {
        var produto = new Produto(1, "Teclado", 100m, 10);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartão", new List<ItemPedido> { new ItemPedido(produto, 1) });

        if (status == StatusPagamento.Recusado) {
            pedido.AtualizarStatusPagamento(StatusPagamento.Recusado);
        }

        return pedido;
    }

    [Fact]
    public void ListarPedidos_QuandoExistemPedidos_DeveRetornarPedidosMapeados() {

        // A listagem deve retornar os pedidos com seus dados e itens convertidos para DTOs.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var pedido = new Pedido(
            "Maria",
            "maria@email.com",
            "Cartão",
            new List<ItemPedido> { new ItemPedido(produto, 2) });

        _pedidoRepositoryMock.Setup(r => r.ListarPedidos()).Returns(new List<Pedido> { pedido });

        var resultado = _service.ListarPedidos().ToList();

        var pedidoResultado = Assert.Single(resultado);
        Assert.Equal("Maria", pedidoResultado.ClienteNome);
        Assert.Equal("maria@email.com", pedidoResultado.ClienteEmail);
        Assert.Equal("Cartão", pedidoResultado.FormaPagamento);
        Assert.Single(pedidoResultado.Itens);
        Assert.Equal(1, pedidoResultado.Itens[0].ProdutoId);
        Assert.Equal(2, pedidoResultado.Itens[0].Quantidade);
        Assert.Equal(200m, pedidoResultado.Itens[0].Subtotal);
    }

    [Fact]
    public void ListarPedidos_QuandoNaoExistemPedidos_DeveRetornarListaVazia() {

        // Quando não existem pedidos, a listagem deve retornar uma coleção vazia.
        _pedidoRepositoryMock.Setup(r => r.ListarPedidos()).Returns(new List<Pedido>());

        var resultado = _service.ListarPedidos();

        Assert.Empty(resultado);
    }

    [Fact]
    public void ConsultarPedidoPorId_QuandoPedidoExiste_DeveRetornarPedidoMapeado() {

        // A consulta deve retornar os dados do pedido e seus itens.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var pedido = new Pedido(
            "Maria",
            "maria@email.com",
            "Cartão",
            new List<ItemPedido> { new ItemPedido(produto, 2) });

        _pedidoRepositoryMock.Setup(r => r.ObterPedidoPorId(1)).Returns(pedido);

        var resultado = _service.ConsultarPedidoPorId(1);

        Assert.NotNull(resultado);
        Assert.Equal("Maria", resultado.ClienteNome);
        Assert.Equal(200m, resultado.ValorBruto);
        Assert.Single(resultado.Itens);
        Assert.Equal(100m, resultado.Itens[0].PrecoUnitario);
    }

    [Fact]
    public void ConsultarPedidoPorId_QuandoPedidoNaoExiste_DeveRetornarNulo() {

        // Quando o repositório não encontra o pedido, o serviço deve retornar null.
        _pedidoRepositoryMock.Setup(r => r.ObterPedidoPorId(99)).Returns((Pedido?)null);

        var resultado = _service.ConsultarPedidoPorId(99);

        Assert.Null(resultado);
    }

    [Fact]
    public async Task CriarPedidoAsync_QuandoDadosValidos_DevePersistirPedidoEEnfileirarPagamento() {

        // O pedido deve ser criado com seus itens e enviado para a fila de pagamento somente após a persistência.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var dto = new CriarPedidoDto {
            ClienteNome = "Maria",
            ClienteEmail = "maria@email.com",
            FormaPagamento = "Cartão",
            Itens = new List<CriarItemPedidoDto>
            {
                new() { ProdutoId = 1, Quantidade = 2 }
            }
        };

        Pedido? pedidoPersistido = null;

        _produtoRepositoryMock.Setup(r => r.ObterProdutoPorId(1)).Returns(produto);
        _pedidoTransacaoRepositoryMock
            .Setup(r => r.AdicionarComBaixaDeEstoque(
                It.IsAny<Pedido>(),
                It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>()))
            .Returns((Pedido pedido, IEnumerable<(int ProdutoId, int Quantidade)> _) => {
                pedidoPersistido = pedido;
                return pedido;
            });

        _filaPagamentoMock
            .Setup(f => f.EnfileirarAsync(It.IsAny<int>()))
            .Returns(ValueTask.CompletedTask);

        var resultado = await _service.CriarPedidoAsync(dto);

        Assert.NotNull(pedidoPersistido);
        Assert.Equal("Maria", resultado.ClienteNome);
        Assert.Equal(200m, resultado.ValorBruto);
        Assert.Single(resultado.Itens);

        _pedidoTransacaoRepositoryMock.Verify(r => r.AdicionarComBaixaDeEstoque(
            It.Is<Pedido>(p => p.ClienteNome == "Maria" && p.Itens.Count == 1),
            It.Is<IEnumerable<(int ProdutoId, int Quantidade)>>(itens =>
            itens.Count() == 1 &&
            itens.First().ProdutoId == 1 &&
            itens.First().Quantidade == 2)));

        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(pedidoPersistido.Id), Times.Once);
    }

    [Fact]
    public async Task CriarPedidoAsync_QuandoProdutoNaoExiste_DeveLancarExcecaoSemPersistirPedido() {

        // Se algum produto não existir, a criação deve falhar antes de persistir o pedido ou enfileirar o pagamento.
        var dto = new CriarPedidoDto {
            ClienteNome = "Maria",
            ClienteEmail = "maria@email.com",
            FormaPagamento = "Cartão",
            Itens = new List<CriarItemPedidoDto>
            {
                new() { ProdutoId = 99, Quantidade = 1 }
            }
        };

        _produtoRepositoryMock.Setup(r => r.ObterProdutoPorId(99)).Returns((Produto?)null);

        await Assert.ThrowsAsync<KeyNotFoundException>(() => _service.CriarPedidoAsync(dto));

        _pedidoTransacaoRepositoryMock.Verify(r => r.AdicionarComBaixaDeEstoque(
            It.IsAny<Pedido>(),
            It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>()), Times.Never);

        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CriarPedidoAsync_QuandoTransacaoFalha_DeveLancarExcecaoSemEnfileirarPagamento() {

        // Se a transação não criar o pedido, o serviço deve lançar uma exceção e não enviar o pedido para pagamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var dto = new CriarPedidoDto {
            ClienteNome = "Maria",
            ClienteEmail = "maria@email.com",
            FormaPagamento = "Cartão",
            Itens = new List<CriarItemPedidoDto>
            {
                new() { ProdutoId = 1, Quantidade = 2 }
            }
        };

        _produtoRepositoryMock.Setup(r => r.ObterProdutoPorId(1)).Returns(produto);
        _pedidoTransacaoRepositoryMock
            .Setup(r => r.AdicionarComBaixaDeEstoque(
                It.IsAny<Pedido>(),
                It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>()))
            .Returns((Pedido?)null);

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarPedidoAsync(dto));

        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task CriarPedidoAsync_QuandoFilaDePagamentoFalha_DevePropagarExcecao() {

        // Se a fila falhar depois da persistência, a exceção deve ser propagada ao chamador.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var dto = new CriarPedidoDto {
            ClienteNome = "Maria",
            ClienteEmail = "maria@email.com",
            FormaPagamento = "Cartão",
            Itens = new List<CriarItemPedidoDto>
            {
                new() { ProdutoId = 1, Quantidade = 1 }
            }
        };

        _produtoRepositoryMock.Setup(r => r.ObterProdutoPorId(1)).Returns(produto);
        _pedidoTransacaoRepositoryMock
            .Setup(r => r.AdicionarComBaixaDeEstoque(
                It.IsAny<Pedido>(),
                It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>()))
            .Returns((Pedido pedido, IEnumerable<(int ProdutoId, int Quantidade)> _) => pedido);

        _filaPagamentoMock
            .Setup(f => f.EnfileirarAsync(It.IsAny<int>()))
            .ThrowsAsync(new InvalidOperationException("Falha na fila."));

        await Assert.ThrowsAsync<InvalidOperationException>(() => _service.CriarPedidoAsync(dto));

        _pedidoTransacaoRepositoryMock.Verify(r => r.AdicionarComBaixaDeEstoque(
            It.IsAny<Pedido>(),
            It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>()), Times.Once);
    }

    [Fact]
    public async Task ReprocessarPagamentoAsync_QuandoPedidoNaoExiste_DeveRetornarNulo() {

        // Um pedido inexistente não pode ser reenviado para a fila de pagamento.
        _pedidoRepositoryMock.Setup(r => r.ObterPedidoPorId(99)).Returns((Pedido?)null);

        var resultado = await _service.ReprocessarPagamentoAsync(99);

        Assert.Null(resultado);
        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ReprocessarPagamentoAsync_QuandoPagamentoNaoFoiRecusado_DeveLancarExcecao() {

        // Apenas pedidos com pagamento recusado podem ser reprocessados.
        var pedido = CriarPedidoComStatus(StatusPagamento.Pendente);

        _pedidoRepositoryMock.Setup(r => r.ObterPedidoPorId(1)).Returns(pedido);

        await Assert.ThrowsAsync<InvalidOperationException>(
            () => _service.ReprocessarPagamentoAsync(1));

        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(It.IsAny<Pedido>()), Times.Never);
        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public async Task ReprocessarPagamentoAsync_QuandoPagamentoFoiRecusado_DeveAtualizarStatusEEnfileirar() {

        // Um pagamento recusado deve voltar para pendente, ser persistido e ser reenviado para a fila.
        var pedido = CriarPedidoComStatus(StatusPagamento.Recusado);

        _pedidoRepositoryMock.Setup(r => r.ObterPedidoPorId(1)).Returns(pedido);
        _pedidoRepositoryMock.Setup(r => r.AtualizarPedido(pedido));
        _filaPagamentoMock
            .Setup(f => f.EnfileirarAsync(1))
            .Returns(ValueTask.CompletedTask);

        var resultado = await _service.ReprocessarPagamentoAsync(1);

        Assert.NotNull(resultado);
        Assert.Equal(StatusPagamento.Pendente, resultado.StatusPagamento);
        _pedidoRepositoryMock.Verify(r => r.AtualizarPedido(pedido), Times.Once);
        _filaPagamentoMock.Verify(f => f.EnfileirarAsync(1), Times.Once);
    }
}
