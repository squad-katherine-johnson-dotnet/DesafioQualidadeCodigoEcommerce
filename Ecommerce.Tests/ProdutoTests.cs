using Ecommerce.Domain.Entities;

namespace Ecommerce.Tests;

public class ProdutoTests {

    [Fact]
    public void AtualizarDados_QuandoNomeVazio_DeveLancarExcecao() {

        // Cria um produto com dados válidos.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Tenta atualizar o produto com o nome vazio.
        var acao = () => produto.AtualizarDados("", 150m, 5);

        // Verifica se a atualização é rejeitada.
        Assert.Throws<ArgumentException>(acao);
    }

    [Fact]
    public void AtualizarDados_QuandoPrecoZero_DeveLancarExcecao() {

        // Cria um produto com dados válidos.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Tenta atualizar o produto com preço zero.
        var acao = () => produto.AtualizarDados("Mouse", 0m, 5);

        // Verifica se o preço inválido é rejeitado.
        Assert.Throws<ArgumentException>(acao);
    }

    [Fact]
    public void AtualizarDados_QuandoEstoqueNegativo_DeveLancarExcecao() {

        // Cria um produto com dados válidos.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Tenta atualizar o produto com estoque negativo.
        var acao = () => produto.AtualizarDados("Mouse", 150m, -1);

        // Verifica se o estoque inválido é rejeitado.
        Assert.Throws<ArgumentException>(acao);
    }

    [Fact]
    public void AtualizarDados_QuandoDadosValidos_DeveAtualizarProduto() {

        // Cria um produto com dados iniciais.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Atualiza os dados com valores válidos.
        produto.AtualizarDados("Mouse", 150m, 5);

        // Verifica se os dados foram atualizados.
        Assert.Equal("Mouse", produto.Nome);
        Assert.Equal(150m, produto.Preco);
        Assert.Equal(5, produto.Estoque);
    }

    [Fact]
    public void RegistrarEntradaEstoque_QuandoQuantidadeValida_DeveAumentarEstoque() {

        // Cria um produto com estoque inicial de 10 unidades.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Registra a entrada de 5 unidades.
        produto.RegistrarEntradaEstoque(5);

        // Verifica se o estoque passou para 15 unidades.
        Assert.Equal(15, produto.Estoque);
    }

    [Fact]
    public void RegistrarEntradaEstoque_QuandoQuantidadeZero_DeveLancarExcecao() {

        // Cria um produto com estoque inicial de 10 unidades.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Tenta registrar a entrada de zero unidades.
        var acao = () => produto.RegistrarEntradaEstoque(0);

        // Verifica se a quantidade inválida é rejeitada.
        Assert.Throws<ArgumentOutOfRangeException>(acao);
    }

    [Fact]
    public void RegistrarSaidaEstoque_QuandoQuantidadeValida_DeveDiminuirEstoque() {

        // Cria um produto com estoque inicial de 10 unidades.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Registra a saída de 3 unidades.
        produto.RegistrarSaidaEstoque(3);

        // Verifica se o estoque passou para 7 unidades.
        Assert.Equal(7, produto.Estoque);
    }

    [Fact]
    public void RegistrarSaidaEstoque_QuandoQuantidadeMaiorQueEstoque_DeveLancarExcecao() {

        // Cria um produto com estoque inicial de 10 unidades.
        var produto = new Produto(1, "Teclado", 100m, 10);

        // Tenta retirar mais unidades do que estão disponíveis.
        var acao = () => produto.RegistrarSaidaEstoque(11);

        // Verifica se a operação é rejeitada por falta de estoque.
        Assert.Throws<InvalidOperationException>(acao);
    }
}

