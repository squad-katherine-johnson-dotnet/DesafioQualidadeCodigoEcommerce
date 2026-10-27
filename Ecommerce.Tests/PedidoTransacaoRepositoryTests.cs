using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Context;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests;

public class PedidoTransacaoRepositoryTests {

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveBaixarEstoqueEPersistirPedido() {

        // Prepara um banco em memória com um produto que possui estoque suficiente.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 2);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Registra o pedido e solicita a baixa de duas unidades.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)> { (1, 2) });

        // Confirma que o pedido foi salvo e o estoque foi reduzido.
        Assert.NotNull(resultado);
        Assert.NotEqual(0, resultado.Id);
        Assert.Equal(1, contexto.Pedidos.Count());
        Assert.Equal(8, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveRetornarNuloQuandoEstoqueForInsuficiente() {

        // Prepara um produto com apenas duas unidades disponíveis.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 2);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 3);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Tenta registrar um pedido que exige mais unidades do que o estoque disponível.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)> { (1, 3) });

        // Confirma que o pedido não foi salvo e o estoque permaneceu intacto.
        Assert.Null(resultado);
        Assert.Empty(contexto.Pedidos);
        Assert.Equal(2, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveReverterBaixasQuandoUmProdutoNaoTiverEstoqueSuficiente() {

        // Prepara dois produtos, sendo que o segundo não possui estoque suficiente.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto1 = new Produto(1, "Teclado", 100m, 10);
        var produto2 = new Produto(2, "Mouse", 50m, 1);

        contexto.Produtos.AddRange(produto1, produto2);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto1, 2);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Solicita a baixa dos dois produtos, mas o segundo não atende à quantidade pedida.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido,
            new List<(int ProdutoId, int Quantidade)> { (1, 2), (2, 3) });

        // Confirma que a transação desfez a primeira baixa e não salvou o pedido.
        Assert.Null(resultado);
        Assert.Empty(contexto.Pedidos);
        Assert.Equal(10, ObterEstoque(contexto, 1));
        Assert.Equal(1, ObterEstoque(contexto, 2));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveSomarQuantidadesDoMesmoProduto() {

        // Prepara um produto com estoque suficiente para duas solicitações de baixa.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 3);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Solicita duas baixas do mesmo produto, que devem ser somadas.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)> { (1, 2), (1, 3) });

        // Confirma que a baixa total foi de cinco unidades.
        Assert.NotNull(resultado);
        Assert.Equal(1, contexto.Pedidos.Count());
        Assert.Equal(5, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveRetornarNuloQuandoListaDeItensEstiverVazia() {

        // Prepara um banco em memória sem movimentações de estoque.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 1);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Tenta registrar o pedido sem informar itens para a baixa de estoque.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)>());

        // Confirma que a operação foi recusada sem alterar os dados.
        Assert.Null(resultado);
        Assert.Empty(contexto.Pedidos);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void AdicionarComBaixaDeEstoque_DeveRetornarNuloQuandoIdentificadorOuQuantidadeForInvalido(
        int produtoId,
        int quantidade) {

        // Prepara um banco com um produto e estoque inicial conhecido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 1);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Tenta executar a operação com identificador ou quantidade inválidos.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)> { (produtoId, quantidade) });

        // Confirma que a operação não salvou o pedido nem alterou o estoque.
        Assert.Null(resultado);
        Assert.Empty(contexto.Pedidos);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveRetornarNuloQuandoProdutoNaoExistir() {

        // Prepara um banco sem o produto solicitado na baixa.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 1);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Tenta baixar o estoque de um identificador que não está cadastrado.
        var resultado = repositorio.AdicionarComBaixaDeEstoque(
            pedido, new List<(int ProdutoId, int Quantidade)> { (99, 1) });

        // Confirma que o pedido não foi salvo e o estoque existente não mudou.
        Assert.Null(resultado);
        Assert.Empty(contexto.Pedidos);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveLancarExcecaoQuandoPedidoForNulo() {

        // Prepara o banco e o repositório sem necessidade de cadastrar produtos.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new PedidoTransacaoRepository(contexto);

        // Confirma que informar um pedido nulo gera a exceção esperada.
        Assert.Throws<ArgumentNullException>(() =>
            repositorio.AdicionarComBaixaDeEstoque(null!, new List<(int ProdutoId, int Quantidade)> { (1, 1) }));
    }

    [Fact]
    public void AdicionarComBaixaDeEstoque_DeveLancarExcecaoQuandoListaForNula() {

        // Prepara um banco em memória e um pedido válido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 1);
        var repositorio = new PedidoTransacaoRepository(contexto);

        // Confirma que informar uma lista nula gera a exceção esperada.
        Assert.Throws<ArgumentNullException>(() => repositorio.AdicionarComBaixaDeEstoque(pedido, null!));
    }

    private static SqliteConnection CriarConexao() {

        // Abre uma conexão que mantém o banco SQLite em memória durante o teste.
        var conexao = new SqliteConnection("Data Source=:memory:");
        conexao.Open();
        return conexao;
    }

    private static AppDbContext CriarContexto(SqliteConnection conexao) {

        // Cria o contexto e as tabelas necessárias para o cenário.
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conexao)
            .Options;

        var contexto = new AppDbContext(opcoes);
        contexto.Database.EnsureCreated();
        return contexto;
    }

    private static Pedido CriarPedido(Produto produto, int quantidade) {

        // Monta um pedido válido para os cenários de teste.
        var item = new ItemPedido(produto, quantidade);

        return new Pedido("Maria", "maria@email.com", "Cartão", new List<ItemPedido> { item });
    }

    private static int ObterEstoque(AppDbContext contexto, int produtoId) {

        // Consulta o estoque diretamente no banco para verificar o valor persistido.
        return contexto.Produtos
            .AsNoTracking()
            .Single(p => p.Id == produtoId)
            .Estoque;
    }
}
