using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Context;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests;

public class PedidoRepositoryTests {

    [Fact]
    public void ObterPedidoPorId_DeveRetornarPedidoComSeusItens() {

        // Cadastra um produto e um pedido com um item associado.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 2);
        contexto.Pedidos.Add(pedido);
        contexto.SaveChanges();

        var repositorio = new PedidoRepository(contexto);

        // Busca o pedido pelo identificador.
        var resultado = repositorio.ObterPedidoPorId(pedido.Id);

        // Confirma que o pedido e seus itens foram retornados.
        Assert.NotNull(resultado);
        Assert.Equal(pedido.Id, resultado.Id);
        Assert.Single(resultado.Itens);
    }

    [Fact]
    public void ObterPedidoPorId_DeveRetornarNuloQuandoPedidoNaoExistir() {

        // Prepara um banco sem pedidos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new PedidoRepository(contexto);

        // Busca um identificador inexistente.
        var resultado = repositorio.ObterPedidoPorId(999);

        // Confirma que nenhum pedido foi encontrado.
        Assert.Null(resultado);
    }

    [Fact]
    public void ListarPedidos_DeveRetornarPedidosComSeusItens() {

        // Cadastra dois pedidos associados a um produto.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido1 = CriarPedido(produto, 1);
        var pedido2 = CriarPedido(produto, 2);

        contexto.Pedidos.AddRange(pedido1, pedido2);
        contexto.SaveChanges();

        var repositorio = new PedidoRepository(contexto);

        // Lista os pedidos cadastrados.
        var resultado = repositorio.ListarPedidos().ToList();

        // Confirma que os pedidos e seus itens foram retornados.
        Assert.Equal(2, resultado.Count);
        Assert.All(resultado, pedido => Assert.NotEmpty(pedido.Itens));
    }

    [Fact]
    public void ListarPedidos_DeveRetornarListaVaziaQuandoNaoExistiremPedidos() {

        // Prepara um banco sem pedidos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new PedidoRepository(contexto);

        // Consulta a listagem de pedidos.
        var resultado = repositorio.ListarPedidos();

        // Confirma que a listagem está vazia.
        Assert.Empty(resultado);
    }

    [Fact]
    public void AtualizarPedido_DevePersistirAlteracoesDoPedido() {

        // Cadastra um pedido para testar sua atualização.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        contexto.Produtos.Add(produto);
        contexto.SaveChanges();

        var pedido = CriarPedido(produto, 1);
        contexto.Pedidos.Add(pedido);
        contexto.SaveChanges();

        var pedidoId = pedido.Id;
        var repositorio = new PedidoRepository(contexto);

        // Atualiza o status de pagamento do pedido.
        pedido.AtualizarStatusPagamento(
            Ecommerce.Domain.Enums.StatusPagamento.Aprovado);

        repositorio.AtualizarPedido(pedido);

        // Confirma que o novo status foi persistido no banco.
        contexto.ChangeTracker.Clear();

        var pedidoPersistido = contexto.Pedidos
            .AsNoTracking()
            .Single(p => p.Id == pedidoId);

        Assert.Equal(
            Ecommerce.Domain.Enums.StatusPagamento.Aprovado,
            pedidoPersistido.StatusPagamento);
    }

    [Fact]
    public void AtualizarPedido_DeveLancarExcecaoQuandoPedidoForNulo() {

        // Prepara o banco e o repositório para validar o argumento recebido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new PedidoRepository(contexto);

        // Confirma que informar um pedido nulo gera a exceção esperada.
        Assert.Throws<ArgumentNullException>(() =>
            repositorio.AtualizarPedido(null!));
    }

    private static SqliteConnection CriarConexao() {

        // Mantém o banco SQLite em memória disponível durante o teste.
        var conexao = new SqliteConnection("Data Source=:memory:");
        conexao.Open();
        return conexao;
    }

    private static AppDbContext CriarContexto(SqliteConnection conexao) {

        // Cria as tabelas necessárias para o cenário.
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conexao)
            .Options;

        var contexto = new AppDbContext(opcoes);
        contexto.Database.EnsureCreated();
        return contexto;
    }

    private static Pedido CriarPedido(Produto produto, int quantidade) {

        // Monta um pedido válido para os testes do repositório.
        var item = new ItemPedido(produto, quantidade);

        return new Pedido("Maria", "maria@email.com", "Cartão", new List<ItemPedido> { item });
    }
}
