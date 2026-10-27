using Ecommerce.Application.DTOs;
using Ecommerce.Application.Services;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;
using Moq;

namespace Ecommerce.Tests;

public class ProdutoServiceTests {
    private readonly Mock<IProdutoRepository> _repositoryMock;
    private readonly ProdutoService _service;

    public ProdutoServiceTests() {

        // Prepara um repositório simulado para testar o serviço sem acessar o banco de dados.
        _repositoryMock = new Mock<IProdutoRepository>();
        _service = new ProdutoService(_repositoryMock.Object);
    }

    [Fact]
    public void ListarProdutos_QuandoExistemProdutos_DeveRetornarProdutosMapeados() {

        // Verifica se os produtos do repositório são convertidos corretamente para DTOs.
        var produtos = new List<Produto>
        {
            new Produto(1, "Teclado", 100m, 10),
            new Produto(2, "Mouse", 50m, 20)
        };

        _repositoryMock
            .Setup(r => r.ListarProdutos())
            .Returns(produtos);

        var resultado = _service.ListarProdutos().ToList();

        Assert.Equal(2, resultado.Count);
        Assert.Equal(1, resultado[0].Id);
        Assert.Equal("Teclado", resultado[0].Nome);
        Assert.Equal(100m, resultado[0].Preco);
        Assert.Equal(10, resultado[0].Estoque);
        Assert.Equal("Mouse", resultado[1].Nome);
    }

    [Fact]
    public void ListarProdutos_QuandoNaoExistemProdutos_DeveRetornarListaVazia() {

        // Verifica se a listagem retorna vazia quando não há produtos cadastrados.
        _repositoryMock
            .Setup(r => r.ListarProdutos())
            .Returns(new List<Produto>());

        var resultado = _service.ListarProdutos();

        Assert.Empty(resultado);
    }

    [Fact]
    public void ConsultarProdutoPorId_QuandoProdutoExiste_DeveRetornarProdutoMapeado() {

        // Verifica se a consulta retorna os dados do produto encontrado.
        var produto = new Produto(1, "Teclado", 100m, 10);

        _repositoryMock
            .Setup(r => r.ObterProdutoPorId(1))
            .Returns(produto);

        var resultado = _service.ConsultarProdutoPorId(1);

        Assert.NotNull(resultado);
        Assert.Equal(1, resultado.Id);
        Assert.Equal("Teclado", resultado.Nome);
        Assert.Equal(100m, resultado.Preco);
        Assert.Equal(10, resultado.Estoque);
    }

    [Fact]
    public void ConsultarProdutoPorId_QuandoProdutoNaoExiste_DeveRetornarNulo() {

        // Verifica se a consulta retorna nulo quando o produto não é encontrado.
        _repositoryMock
            .Setup(r => r.ObterProdutoPorId(99))
            .Returns((Produto?)null);

        var resultado = _service.ConsultarProdutoPorId(99);

        Assert.Null(resultado);
    }

    [Fact]
    public void CriarProduto_QuandoDadosValidos_DeveAdicionarProdutoERetornarDto() {

        // Verifica se o serviço cria a entidade, chama o repositório e retorna os dados cadastrados.
        var dto = new CriarProdutoDto {
            Nome = "Teclado",
            Preco = 100m,
            Estoque = 10
        };

        _repositoryMock
            .Setup(r => r.AdicionarProduto(It.IsAny<Produto>()))
            .Returns((Produto produto) => new Produto(1, produto.Nome, produto.Preco, produto.Estoque));

        var resultado = _service.CriarProduto(dto);

        Assert.Equal(1, resultado.Id);
        Assert.Equal("Teclado", resultado.Nome);
        Assert.Equal(100m, resultado.Preco);
        Assert.Equal(10, resultado.Estoque);

        _repositoryMock.Verify(
            r => r.AdicionarProduto(It.Is<Produto>(p =>
                p.Nome == "Teclado" &&
                p.Preco == 100m &&
                p.Estoque == 10)),
            Times.Once);
    }

    [Fact]
    public void AtualizarProduto_QuandoProdutoExiste_DeveAtualizarNomeEPreco() {

        // Verifica se a atualização altera nome e preço, preservando o estoque atual.
        var produto = new Produto(1, "Teclado antigo", 80m, 15);
        var dto = new AtualizarProdutoDto {
            Nome = "Teclado novo",
            Preco = 120m
        };

        _repositoryMock
            .Setup(r => r.ObterProdutoPorId(1))
            .Returns(produto);

        _repositoryMock
            .Setup(r => r.AtualizarProduto(It.IsAny<Produto>()))
            .Returns(true);

        var resultado = _service.AtualizarProduto(1, dto);

        Assert.True(resultado);
        Assert.Equal("Teclado novo", produto.Nome);
        Assert.Equal(120m, produto.Preco);
        Assert.Equal(15, produto.Estoque);

        _repositoryMock.Verify(
            r => r.AtualizarProduto(produto),
            Times.Once);
    }

    [Fact]
    public void AtualizarProduto_QuandoProdutoNaoExiste_DeveRetornarFalso() {

        // Verifica se a atualização não chama o repositório para persistência quando o produto não existe.
        _repositoryMock
            .Setup(r => r.ObterProdutoPorId(99))
            .Returns((Produto?)null);

        var dto = new AtualizarProdutoDto {
            Nome = "Teclado novo",
            Preco = 120m
        };

        var resultado = _service.AtualizarProduto(99, dto);

        Assert.False(resultado);

        _repositoryMock.Verify(
            r => r.AtualizarProduto(It.IsAny<Produto>()),
            Times.Never);
    }

    [Fact]
    public void AtualizarProduto_QuandoRepositorioFalha_DeveRetornarFalso() {

        // Verifica se o serviço devolve falso quando o repositório não consegue atualizar o produto.
        var produto = new Produto(1, "Teclado", 100m, 10);

        _repositoryMock
            .Setup(r => r.ObterProdutoPorId(1))
            .Returns(produto);

        _repositoryMock
            .Setup(r => r.AtualizarProduto(It.IsAny<Produto>()))
            .Returns(false);

        var dto = new AtualizarProdutoDto {
            Nome = "Teclado novo",
            Preco = 120m
        };

        var resultado = _service.AtualizarProduto(1, dto);

        Assert.False(resultado);
    }

    [Fact]
    public void RemoverProduto_QuandoRepositorioRemoveComSucesso_DeveRetornarVerdadeiro() {

        // Verifica se o serviço retorna verdadeiro quando o repositório remove o produto.
        _repositoryMock
            .Setup(r => r.RemoverProduto(1))
            .Returns(true);

        var resultado = _service.RemoverProduto(1);

        Assert.True(resultado);

        _repositoryMock.Verify(
            r => r.RemoverProduto(1),
            Times.Once);
    }

    [Fact]
    public void RemoverProduto_QuandoRepositorioNaoEncontraProduto_DeveRetornarFalso() {

        // Verifica se o serviço retorna falso quando o repositório não remove o produto.
        _repositoryMock
            .Setup(r => r.RemoverProduto(99))
            .Returns(false);

        var resultado = _service.RemoverProduto(99);

        Assert.False(resultado);
    }
}
