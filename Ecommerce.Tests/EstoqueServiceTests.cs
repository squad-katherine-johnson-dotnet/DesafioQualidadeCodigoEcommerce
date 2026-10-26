using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Moq;

namespace Ecommerce.Tests;

public class EstoqueServiceTests {
    private readonly Mock<IProdutoRepository> _repositoryMock;
    private readonly EstoqueService _service;

    public EstoqueServiceTests() {
        _repositoryMock = new Mock<IProdutoRepository>();
        _service = new EstoqueService(_repositoryMock.Object);
    }

    [Fact]
    public void ConsultarEstoque_QuandoExistemProdutos_DeveRetornarProdutosMapeados() {

        // O serviço deve retornar os produtos com seus dados convertidos para DTOs.
        var produtos = new List<Produto>
        {
            new Produto(1, "Teclado", 100m, 10),
            new Produto(2, "Mouse", 50m, 20)
        };

        _repositoryMock.Setup(r => r.ListarProdutos()).Returns(produtos);

        var resultado = _service.ConsultarEstoque().ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal(1, resultado[0].Id);
        Assert.Equal("Teclado", resultado[0].Nome);
        Assert.Equal(100m, resultado[0].Preco);
        Assert.Equal(10, resultado[0].Estoque);
        Assert.Equal("Mouse", resultado[1].Nome);
        Assert.Equal(20, resultado[1].Estoque);
    }

    [Fact]
    public void ConsultarEstoque_QuandoNaoExistemProdutos_DeveRetornarListaVazia() {

        // Quando o repositório não possui produtos, o serviço deve retornar uma coleção vazia.
        _repositoryMock.Setup(r => r.ListarProdutos()).Returns(new List<Produto>());

        var resultado = _service.ConsultarEstoque();

        Assert.Empty(resultado);
    }

    [Fact]
    public void ConsultarEstoquePorProdutoId_QuandoProdutoExiste_DeveRetornarProdutoMapeado() {

        // O produto encontrado deve ser convertido para o DTO de resposta.
        var produto = new Produto(1, "Teclado", 100m, 10);

        _repositoryMock.Setup(r => r.ObterProdutoPorId(1)).Returns(produto);

        var resultado = _service.ConsultarEstoquePorProdutoId(1);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal("Teclado", resultado.Nome);
        Assert.Equal(100m, resultado.Preco);
        Assert.Equal(10, resultado.Estoque);
    }

    [Fact]
    public void ConsultarEstoquePorProdutoId_QuandoProdutoNaoExiste_DeveRetornarNulo() {

        // Quando o produto não é encontrado, o serviço deve retornar null.
        _repositoryMock.Setup(r => r.ObterProdutoPorId(99)).Returns((Produto?)null);

        var resultado = _service.ConsultarEstoquePorProdutoId(99);

        Assert.Null(resultado);
    }

    [Fact]
    public void RegistrarEntradaDeEstoque_QuandoQuantidadeEProdutoSaoValidos_DeveRetornarEstoqueAtualizado() {

        // A entrada deve ser registrada e o serviço deve consultar novamente o estoque atualizado.
        var produtoAtual = new Produto(1, "Teclado", 100m, 15);

        _repositoryMock.SetupSequence(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 10))
            .Returns(produtoAtual);

        _repositoryMock.Setup(r => r.RegistrarEntradaEstoque(1, 5)).Returns(1);

        var resultado = _service.RegistrarEntradaDeEstoque(1, 5);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal("Teclado", resultado.Nome);
        Assert.Equal(15, resultado.Estoque);
        _repositoryMock.Verify(r => r.RegistrarEntradaEstoque(1, 5), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarEntradaDeEstoque_QuandoQuantidadeNaoEPositiva_DeveLancarExcecao(int quantidade) {

        // A entrada não pode utilizar quantidades iguais ou inferiores a zero.
        var excecao = Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.RegistrarEntradaDeEstoque(1, quantidade));

        Assert.Equal("quantidade", excecao.ParamName);
        _repositoryMock.Verify(r => r.ObterProdutoPorId(It.IsAny<int>()), Times.Never);
        _repositoryMock.Verify(r => r.RegistrarEntradaEstoque(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RegistrarEntradaDeEstoque_QuandoProdutoNaoExiste_DeveRetornarNulo() {

        // A entrada não deve ser registrada se o produto não existir.
        _repositoryMock.Setup(r => r.ObterProdutoPorId(99)).Returns((Produto?)null);

        var resultado = _service.RegistrarEntradaDeEstoque(99, 5);

        Assert.Null(resultado);
        _repositoryMock.Verify(r => r.RegistrarEntradaEstoque(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RegistrarEntradaDeEstoque_QuandoRepositorioNaoAlteraLinhas_DeveLancarExcecao() {

        // Se o repositório não conseguir registrar a entrada, o serviço deve informar a falha.
        _repositoryMock.Setup(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 10));
        _repositoryMock.Setup(r => r.RegistrarEntradaEstoque(1, 5)).Returns(0);

        Assert.Throws<InvalidOperationException>(
            () => _service.RegistrarEntradaDeEstoque(1, 5));
    }

    [Fact]
    public void RegistrarEntradaDeEstoque_QuandoProdutoNaoPodeSerRecuperadoAposAtualizacao_DeveLancarExcecao() {

        // Se a entrada for registrada, mas o produto não puder ser recuperado depois, o serviço deve informar a inconsistência.
        _repositoryMock.SetupSequence(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 10))
            .Returns((Produto?)null);
        _repositoryMock.Setup(r => r.RegistrarEntradaEstoque(1, 5)).Returns(1);

        Assert.Throws<InvalidOperationException>(
            () => _service.RegistrarEntradaDeEstoque(1, 5));
    }

    [Fact]
    public void RegistrarSaidaDeEstoque_QuandoQuantidadeEEstoqueSaoValidos_DeveRetornarEstoqueAtualizado() {

        // A saída deve ser registrada e o serviço deve retornar o estoque após a operação.
        _repositoryMock.SetupSequence(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 10))
            .Returns(new Produto(1, "Teclado", 100m, 7));
        _repositoryMock.Setup(r => r.BaixarEstoqueSeDisponivel(1, 3)).Returns(1);

        var resultado = _service.RegistrarSaidaDeEstoque(1, 3);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal("Teclado", resultado.Nome);
        Assert.Equal(7, resultado.Estoque);
        _repositoryMock.Verify(r => r.BaixarEstoqueSeDisponivel(1, 3), Times.Once);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarSaidaDeEstoque_QuandoQuantidadeNaoEPositiva_DeveLancarExcecao(int quantidade) {

        // A saída não pode utilizar quantidades iguais ou inferiores a zero.
        var excecao = Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.RegistrarSaidaDeEstoque(1, quantidade));

        Assert.Equal("quantidade", excecao.ParamName);
        _repositoryMock.Verify(r => r.ObterProdutoPorId(It.IsAny<int>()), Times.Never);
        _repositoryMock.Verify(r => r.BaixarEstoqueSeDisponivel(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RegistrarSaidaDeEstoque_QuandoProdutoNaoExiste_DeveRetornarNulo() {

        // A saída não deve ser registrada se o produto não existir.
        _repositoryMock.Setup(r => r.ObterProdutoPorId(99)).Returns((Produto?)null);

        var resultado = _service.RegistrarSaidaDeEstoque(99, 5);

        Assert.Null(resultado);
        _repositoryMock.Verify(r => r.BaixarEstoqueSeDisponivel(It.IsAny<int>(), It.IsAny<int>()), Times.Never);
    }

    [Fact]
    public void RegistrarSaidaDeEstoque_QuandoEstoqueEInsuficiente_DeveLancarExcecao() {

        // Se o repositório não conseguir baixar o estoque, o serviço deve informar a falha.
        _repositoryMock.Setup(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 2));
        _repositoryMock.Setup(r => r.BaixarEstoqueSeDisponivel(1, 5)).Returns(0);

        Assert.Throws<InvalidOperationException>(
            () => _service.RegistrarSaidaDeEstoque(1, 5));
    }

    [Fact]
    public void RegistrarSaidaDeEstoque_QuandoProdutoNaoPodeSerRecuperadoAposAtualizacao_DeveLancarExcecao() {

        // Se a saída for registrada, mas o produto não puder ser recuperado depois, o serviço deve informar a inconsistência.
        _repositoryMock.SetupSequence(r => r.ObterProdutoPorId(1))
            .Returns(new Produto(1, "Teclado", 100m, 10))
            .Returns((Produto?)null);
        _repositoryMock.Setup(r => r.BaixarEstoqueSeDisponivel(1, 3)).Returns(1);

        Assert.Throws<InvalidOperationException>(
            () => _service.RegistrarSaidaDeEstoque(1, 3));
    }

    [Fact]
    public void ListarProdutosAbaixoDoEstoqueMinimo_DeveRetornarSomenteProdutosAbaixoDoLimite() {

        // Apenas produtos com estoque estritamente inferior ao limite devem ser retornados.
        var produtos = new List<Produto>
        {
            new Produto(1, "Teclado", 100m, 2),
            new Produto(2, "Mouse", 50m, 5),
            new Produto(3, "Monitor", 800m, 8)
        };

        _repositoryMock.Setup(r => r.ListarProdutos()).Returns(produtos);

        var resultado = _service.ListarProdutosAbaixoDoEstoqueMinimo(5).ToList();

        var produto = Assert.Single(resultado);
        Assert.Equal(1, produto.Id);
        Assert.Equal("Teclado", produto.Nome);
        Assert.Equal(2, produto.Estoque);
    }

    [Fact]
    public void ListarProdutosAbaixoDoEstoqueMinimo_QuandoLimiteForZero_DeveRetornarListaVazia() {

        // Com limite zero, nenhum estoque inteiro não negativo fica abaixo do limite.
        _repositoryMock.Setup(r => r.ListarProdutos()).Returns(new List<Produto>
        {
            new Produto(1, "Teclado", 100m, 0),
            new Produto(2, "Mouse", 50m, 5)
        });

        var resultado = _service.ListarProdutosAbaixoDoEstoqueMinimo(0);

        Assert.Empty(resultado);
    }

    [Fact]
    public void ListarProdutosAbaixoDoEstoqueMinimo_QuandoLimiteForNegativo_DeveLancarExcecao() {

        // O limite mínimo não pode ser negativo.
        var excecao = Assert.Throws<ArgumentOutOfRangeException>(
            () => _service.ListarProdutosAbaixoDoEstoqueMinimo(-1));

        Assert.Equal("quantidadeMinima", excecao.ParamName);
        _repositoryMock.Verify(r => r.ListarProdutos(), Times.Never);
    }

    [Fact]
    public void RegistrarSaidasDeEstoqueEmLote_QuandoRepositorioProcessaComSucesso_DeveRetornarVerdadeiro() {

        // O serviço deve encaminhar os itens ao repositório e retornar o resultado da operação.
        var itens = new List<(int ProdutoId, int Quantidade)>
        {
            (1, 2),
            (2, 3)
        };

        _repositoryMock.Setup(r => r.BaixarEstoqueEmLote(
            It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>())).Returns(true);

        var resultado = _service.RegistrarSaidasDeEstoqueEmLote(itens);

        Assert.True(resultado);
        _repositoryMock.Verify(r => r.BaixarEstoqueEmLote(
            It.Is<IEnumerable<(int ProdutoId, int Quantidade)>>(lista =>
                lista.SequenceEqual(itens))), Times.Once);
    }

    [Fact]
    public void RegistrarSaidasDeEstoqueEmLote_QuandoRepositorioFalha_DeveRetornarFalso() {

        // Se o repositório não conseguir processar o lote, o serviço deve retornar false.
        var itens = new List<(int ProdutoId, int Quantidade)> { (1, 2) };

        _repositoryMock.Setup(r => r.BaixarEstoqueEmLote(
            It.IsAny<IEnumerable<(int ProdutoId, int Quantidade)>>())).Returns(false);

        var resultado = _service.RegistrarSaidasDeEstoqueEmLote(itens);

        Assert.False(resultado);
    }
}
