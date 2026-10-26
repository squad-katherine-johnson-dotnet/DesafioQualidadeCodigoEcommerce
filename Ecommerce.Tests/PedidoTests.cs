using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;

namespace Ecommerce.Tests;

public class PedidoTests {

    [Fact]
    public void Construtor_QuandoNomeVazio_DeveLancarExcecao() {

        // Verifica se o pedido rejeita um nome de cliente vazio.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);

        Assert.Throws<ArgumentException>(() => new Pedido("", "maria@email.com", "Cartao", new[] { item }));
    }

    [Fact]
    public void Construtor_QuandoEmailVazio_DeveLancarExcecao() {

        // Verifica se o pedido rejeita um e-mail vazio.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);

        Assert.Throws<ArgumentException>(() => new Pedido("Maria", "", "Cartao", new[] { item }));
    }

    [Fact]
    public void Construtor_QuandoFormaPagamentoVazia_DeveLancarExcecao() {

        // Verifica se o pedido rejeita uma forma de pagamento vazia.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);

        Assert.Throws<ArgumentException>(() => new Pedido("Maria", "maria@email.com", "", new[] { item }));
    }

    [Fact]
    public void Construtor_QuandoListaDeItensNula_DeveLancarExcecao() {

        // Verifica se o pedido rejeita uma lista de itens nula.
        Assert.Throws<ArgumentNullException>(
            () => new Pedido("Maria", "maria@email.com", "Cartao", null!));
    }

    [Fact]
    public void Construtor_QuandoListaDeItensVazia_DeveLancarExcecao() {

        // Verifica se o pedido exige pelo menos um item.
        Assert.Throws<ArgumentException>(
            () => new Pedido("Maria", "maria@email.com", "Cartao", Array.Empty<ItemPedido>()));
    }


    [Fact]
    public void Construtor_QuandoPedidoValido_DeveCalcularValorBrutoEValorTotal() {

        // Verifica se o pedido soma os subtotais dos itens e mantém o valor total quando não há desconto.
        var produto1 = new Produto(1, "Teclado", 100m, 10);
        var produto2 = new Produto(2, "Mouse", 50m, 10);
        var itens = new[]
        {
        new ItemPedido(produto1, 2),
        new ItemPedido(produto2, 1)
    };

        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", itens);

        Assert.Equal(250m, pedido.ValorBruto);
        Assert.Equal(0m, pedido.ValorDesconto);
        Assert.Equal(250m, pedido.ValorTotal);
    }

    [Fact]
    public void Construtor_QuandoValorBrutoMaiorQue500_DeveAplicarDescontoDe10PorCento() {

        // Verifica se pedidos acima de R$ 500 recebem desconto de 10%.
        var produto = new Produto(1, "Monitor", 600m, 10);
        var item = new ItemPedido(produto, 1);

        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        Assert.Equal(600m, pedido.ValorBruto);
        Assert.Equal(60m, pedido.ValorDesconto);
        Assert.Equal(540m, pedido.ValorTotal);
    }

    [Fact]
    public void Construtor_QuandoClienteVip_DeveAplicarDescontoDe20PorCento() {

        // Verifica se o e-mail contendo "vip" ativa o desconto de 20%.
        var produto = new Produto(1, "Monitor", 600m, 10);
        var item = new ItemPedido(produto, 1);

        var pedido = new Pedido("Maria", "maria.vip@email.com", "Cartao", new[] { item });

        Assert.Equal(600m, pedido.ValorBruto);
        Assert.Equal(120m, pedido.ValorDesconto);
        Assert.Equal(480m, pedido.ValorTotal);
    }

    [Fact]
    public void Construtor_QuandoEmailVipContemVipEmMaiusculas_DeveAplicarDescontoDe20PorCento() {

        // Verifica se a identificação de cliente VIP não diferencia letras maiúsculas e minúsculas.
        var produto = new Produto(1, "Monitor", 600m, 10);
        var item = new ItemPedido(produto, 1);

        var pedido = new Pedido("Maria", "maria.VIP@email.com", "Cartao", new[] { item });

        Assert.Equal(120m, pedido.ValorDesconto);
        Assert.Equal(480m, pedido.ValorTotal);
    }

    [Fact]
    public void Construtor_QuandoValorBrutoIgualA500EClienteNaoVip_NaoDeveAplicarDesconto() {

        // Verifica se o desconto por valor não é aplicado quando o pedido custa exatamente R$ 500.
        var produto = new Produto(1, "Monitor", 500m, 10);
        var item = new ItemPedido(produto, 1);

        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        Assert.Equal(500m, pedido.ValorBruto);
        Assert.Equal(0m, pedido.ValorDesconto);
        Assert.Equal(500m, pedido.ValorTotal);
    }

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

    [Fact]
    public void AtualizarStatusPagamento_QuandoPendente_DeveImpedirRetornoParaPendente() {

        // Verifica se um pagamento pendente não pode ser atualizado para pendente novamente.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        Assert.Throws<InvalidOperationException>(
            () => pedido.AtualizarStatusPagamento(StatusPagamento.Pendente));
    }

    [Fact]
    public void AtualizarStatusPagamento_QuandoRecusado_DeveImpedirNovaRecusa() {

        // Verifica se um pagamento recusado não pode ser recusado novamente.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Recusado);

        Assert.Throws<InvalidOperationException>(
            () => pedido.AtualizarStatusPagamento(StatusPagamento.Recusado));
    }

    [Fact]
    public void AtualizarStatusLogistica_QuandoPendente_DeveImpedirRetornoParaPendente() {

        // Verifica se uma logística pendente não pode voltar para pendente sem ter falhado.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        Assert.Throws<InvalidOperationException>(
            () => pedido.AtualizarStatusLogistica(StatusLogistica.Pendente));
    }

    [Fact]
    public void AtualizarStatusLogistica_QuandoEnviado_DeveImpedirNovaAlteracao() {

        // Verifica se uma logística já enviada não pode ser alterada novamente.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);
        pedido.AtualizarStatusLogistica(StatusLogistica.Enviado, "BR123456789");

        Assert.Throws<InvalidOperationException>(
            () => pedido.AtualizarStatusLogistica(StatusLogistica.Falhou));
    }

    [Fact]
    public void AtualizarStatusLogistica_QuandoFalhou_DevePermitirRetornoParaPendente() {

        // Verifica se uma logística que falhou pode voltar para pendente para permitir reprocessamento.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 1);
        var pedido = new Pedido("Maria", "maria@email.com", "Cartao", new[] { item });

        pedido.AtualizarStatusPagamento(StatusPagamento.Aprovado);
        pedido.AtualizarStatusLogistica(StatusLogistica.Falhou);
        pedido.AtualizarStatusLogistica(StatusLogistica.Pendente);

        Assert.Equal(StatusLogistica.Pendente, pedido.StatusLogistica);
    }

}
