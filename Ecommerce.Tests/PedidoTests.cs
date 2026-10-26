using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Tests;

public class PedidoTests {

    [Fact]
    public void AtualizarStatusPagamento_QuandoPendente_DevePermitirRecusa() {

        // Cria um produto, um item e um pedido com pagamento pendente.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        // Atualiza o pagamento para recusado.
        pedido.AtualizarStatusPagamento(StatusPagamento.Recusado);

        // Verifica se o pagamento ficou com o status recusado.
        Assert.Equal(StatusPagamento.Recusado, pedido.StatusPagamento);
    }

    [Fact]
    public void AtualizarStatusPagamento_QuandoAprovado_DeveImpedirAlteracao() {

        // Cria um pedido e aprova o pagamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);

        // Tenta alterar um pagamento que já foi aprovado.
        var acao = () => pedido.AtualizarStatusPagamento(StatusPagamento.Recusado);

        // Verifica se a alteração é impedida.
        Assert.Throws<InvalidOperationException>(acao);
    }

    [Fact]
    public void AtualizarStatusPagamento_QuandoRecusado_DevePermitirRetornoParaPendente() {

        // Cria um pedido e recusa o pagamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Recusado);

        // Retorna o pagamento recusado para pendente.
        pedido.AtualizarStatusPagamento(StatusPagamento.Pendente);

        // Verifica se o pagamento está pendente para uma nova tentativa.
        Assert.Equal(StatusPagamento.Pendente, pedido.StatusPagamento);
    }

    [Fact]
    public void AtualizarStatusLogistica_QuandoPagamentoPendente_DeveImpedirEnvio() {
        // Cria um pedido com pagamento pendente.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        // Tenta enviar o pedido antes da aprovação do pagamento.
        var acao = () => pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, "BR00000001");

        // Verifica se o envio é impedido.
        Assert.Throws<InvalidOperationException>(acao);
    }

    [Fact]
    public void AtualizarStatusLogistica_SemCodigoRastreio_DeveImpedirEnvio() {

        // Cria um pedido e aprova o pagamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);

        // Tenta enviar o pedido sem informar o código de rastreio.
        var acao = () => pedido.AtualizarStatusLogistica(StatusLogistica.Enviado);

        // Verifica se o envio é impedido pela ausência do código.
        Assert.Throws<ArgumentException>(acao);
    }


    [Fact]
    public void AtualizarStatusLogistica_QuandoPagamentoAprovadoECodigoInformado_DevePermitirEnvio() {

        // Cria um pedido e aprova o pagamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);

        // Envia o pedido informando o código de rastreio.
        pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, "BR00000001");

        // Verifica o status da logística e o código de rastreio.
        Assert.Equal(StatusLogistica.Enviado, pedido.StatusLogistica);
        Assert.Equal("BR00000001", pedido.CodigoRastreio);
    }

}
