using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests;

public class ItemPedidoTests {

    [Fact]
    public void Construtor_QuandoProdutoValidoEQuantidadePositiva_DeveCriarItem() {

        // Verifica se é possível criar um item de pedido com produto válido e quantidade positiva.
        var produto = new Produto(1, "Teclado", 100m, 10);

        var item = new ItemPedido(produto, 2);

        Assert.Equal(1, item.ProdutoId);
        Assert.Equal(2, item.Quantidade);
        Assert.Equal(100m, item.PrecoUnitario);
    }

    [Fact]
    public void Construtor_QuandoQuantidadeZero_DeveLancarExcecao() {

        // Verifica se a criação do item é impedida quando a quantidade é zero.
        var produto = new Produto(1, "Teclado", 100m, 10);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ItemPedido(produto, 0));
    }

    [Fact]
    public void Construtor_QuandoQuantidadeNegativa_DeveLancarExcecao() {

        // Verifica se a criação do item é impedida quando a quantidade é negativa.
        var produto = new Produto(1, "Teclado", 100m, 10);

        Assert.Throws<ArgumentOutOfRangeException>(
            () => new ItemPedido(produto, -1));
    }

    [Fact]
    public void CalcularSubtotal_QuandoQuantidadeValida_DeveRetornarValorCorreto() {

        // Verifica se o subtotal é calculado multiplicando o preço unitário pela quantidade.
        var produto = new Produto(1, "Teclado", 100m, 10);
        var item = new ItemPedido(produto, 3);

        var subtotal = item.CalcularSubtotal();

        Assert.Equal(300m, subtotal);
    }
}

