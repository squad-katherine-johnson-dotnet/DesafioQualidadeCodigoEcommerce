using Ecommerce.Domain.Entities;
using Ecommerce.Infrastructure.Context;
using Ecommerce.Infrastructure.Repositories;
using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;

namespace Ecommerce.Tests;

public class ProdutoRepositoryTests {

    [Fact]
    public void ListarProdutos_DeveRetornarTodosOsProdutos() {

        // Cadastra dois produtos para verificar a listagem.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.AddRange(
            new Produto(1, "Teclado", 100m, 10),
            new Produto(2, "Mouse", 50m, 5));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Confirma que os dois produtos foram retornados.
        var resultado = repositorio.ListarProdutos();

        Assert.Equal(2, resultado.Count());
    }

    [Fact]
    public void ListarProdutos_DeveRetornarListaVaziaQuandoNaoExistiremProdutos() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Confirma que a listagem não retorna elementos.
        Assert.Empty(repositorio.ListarProdutos());
    }

    [Fact]
    public void ObterProdutoPorId_DeveRetornarProdutoExistente() {

        // Cadastra um produto para realizar a busca.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Busca o produto pelo identificador cadastrado.
        var resultado = repositorio.ObterProdutoPorId(1);

        // Confirma que o produto correto foi encontrado.
        Assert.NotNull(resultado);
        Assert.Equal("Teclado", resultado.Nome);
    }

    [Fact]
    public void ObterProdutoPorId_DeveRetornarNuloQuandoProdutoNaoExistir() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Busca um identificador inexistente.
        var resultado = repositorio.ObterProdutoPorId(99);

        // Confirma que nenhum produto foi encontrado.
        Assert.Null(resultado);
    }

    [Fact]
    public void AdicionarProduto_DevePersistirProduto() {

        // Prepara um banco vazio para cadastrar um produto.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(1, "Teclado", 100m, 10);
        var repositorio = new ProdutoRepository(contexto);

        // Cadastra o produto no repositório.
        var resultado = repositorio.AdicionarProduto(produto);

        // Confirma que o produto foi retornado e persistido.
        Assert.Same(produto, resultado);
        Assert.Equal(1, contexto.Produtos.Count());
        Assert.Equal("Teclado", contexto.Produtos.Single().Nome);
    }

    [Fact]
    public void AtualizarProduto_DeveAtualizarNomeEPreco() {

        // Cadastra um produto com os dados originais.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var produtoAtualizado = new Produto(1, "Teclado Mecânico", 150m, 99);
        var repositorio = new ProdutoRepository(contexto);

        // Solicita a atualização dos dados do produto.
        var resultado = repositorio.AtualizarProduto(produtoAtualizado);

        // Confirma que nome e preço mudaram, mas o estoque permaneceu intacto.
        Assert.True(resultado);

        var produtoPersistido = contexto.Produtos
            .AsNoTracking()
            .Single(p => p.Id == 1);

        Assert.Equal("Teclado Mecânico", produtoPersistido.Nome);
        Assert.Equal(150m, produtoPersistido.Preco);
        Assert.Equal(10, produtoPersistido.Estoque);
    }

    [Fact]
    public void AtualizarProduto_DeveRetornarFalsoQuandoProdutoNaoExistir() {

        // Prepara um banco sem o produto que será atualizado.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var produto = new Produto(99, "Teclado", 100m, 10);
        var repositorio = new ProdutoRepository(contexto);

        // Solicita a atualização de um identificador inexistente.
        var resultado = repositorio.AtualizarProduto(produto);

        // Confirma que nenhum produto foi atualizado ou cadastrado.
        Assert.False(resultado);
        Assert.Empty(contexto.Produtos);
    }

    [Fact]
    public void AtualizarProduto_DeveLancarExcecaoQuandoProdutoForNulo() {

        // Prepara o repositório para validar o argumento recebido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Confirma que um argumento nulo gera a exceção esperada.
        Assert.Throws<ArgumentNullException>(() =>
            repositorio.AtualizarProduto(null!));
    }

    [Fact]
    public void RemoverProduto_DeveExcluirProdutoExistente() {

        // Cadastra um produto para testar a remoção.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Remove o produto pelo identificador.
        var resultado = repositorio.RemoverProduto(1);

        // Confirma que a remoção foi realizada.
        Assert.True(resultado);
        Assert.Empty(contexto.Produtos);
    }

    [Fact]
    public void RemoverProduto_DeveRetornarFalsoQuandoProdutoNaoExistir() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Tenta remover um identificador inexistente.
        var resultado = repositorio.RemoverProduto(99);

        // Confirma que a remoção não foi realizada.
        Assert.False(resultado);
    }

    [Fact]
    public void BaixarEstoqueSeDisponivel_DeveReduzirEstoqueQuandoHouverQuantidadeSuficiente() {

        // Cadastra um produto com estoque suficiente.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita a baixa de três unidades.
        var resultado = repositorio.BaixarEstoqueSeDisponivel(1, 3);

        // Confirma que a baixa ocorreu e o estoque foi reduzido.
        Assert.Equal(1, resultado);
        Assert.Equal(7, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void BaixarEstoqueSeDisponivel_DeveRetornarZeroQuandoEstoqueForInsuficiente() {

        // Cadastra um produto com estoque menor que a quantidade solicitada.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 2));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita uma baixa maior que o estoque disponível.
        var resultado = repositorio.BaixarEstoqueSeDisponivel(1, 3);

        // Confirma que a baixa não ocorreu.
        Assert.Equal(0, resultado);
        Assert.Equal(2, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void BaixarEstoqueSeDisponivel_DeveRetornarZeroQuandoProdutoNaoExistir() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Solicita a baixa de um produto inexistente.
        var resultado = repositorio.BaixarEstoqueSeDisponivel(99, 1);

        // Confirma que nenhuma linha foi alterada.
        Assert.Equal(0, resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void BaixarEstoqueSeDisponivel_DeveRetornarZeroQuandoQuantidadeForInvalida(
        int quantidade) {

        // Cadastra um produto com estoque conhecido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita uma baixa com quantidade inválida.
        var resultado = repositorio.BaixarEstoqueSeDisponivel(1, quantidade);

        // Confirma que o estoque não foi alterado.
        Assert.Equal(0, resultado);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void BaixarEstoqueEmLote_DeveBaixarEstoqueDeTodosOsProdutos() {

        // Cadastra dois produtos com estoque suficiente.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.AddRange(
            new Produto(1, "Teclado", 100m, 10),
            new Produto(2, "Mouse", 50m, 5));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita a baixa dos dois produtos na mesma operação.
        var resultado = repositorio.BaixarEstoqueEmLote(
            new List<(int ProdutoId, int Quantidade)>
            {
                (1, 2),
                (2, 3)
            });

        // Confirma que ambas as baixas foram persistidas.
        Assert.True(resultado);
        Assert.Equal(8, ObterEstoque(contexto, 1));
        Assert.Equal(2, ObterEstoque(contexto, 2));
    }

    [Fact]
    public void BaixarEstoqueEmLote_DeveRetornarVerdadeiroQuandoListaEstiverVazia() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Solicita uma baixa em lote sem itens.
        var resultado = repositorio.BaixarEstoqueEmLote(
            new List<(int ProdutoId, int Quantidade)>());

        // Confirma o comportamento definido para uma lista vazia.
        Assert.True(resultado);
        Assert.Empty(contexto.Produtos);
    }

    [Fact]
    public void BaixarEstoqueEmLote_DeveRetornarFalsoQuandoListaForNula() {

        // Prepara o repositório para validar o argumento recebido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Confirma que uma lista nula gera a exceção esperada.
        Assert.Throws<ArgumentNullException>(() =>
            repositorio.BaixarEstoqueEmLote(null!));
    }

    [Theory]
    [InlineData(0, 1)]
    [InlineData(1, 0)]
    [InlineData(-1, 1)]
    [InlineData(1, -1)]
    public void BaixarEstoqueEmLote_DeveRetornarFalsoQuandoItemForInvalido(int produtoId, int quantidade) {

        // Cadastra um produto para conferir se o estoque permanece inalterado.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita uma baixa em lote com identificador ou quantidade inválidos.
        var resultado = repositorio.BaixarEstoqueEmLote(
            new List<(int ProdutoId, int Quantidade)>
            {
                (produtoId, quantidade)
            });

        // Confirma que a operação foi recusada sem alterar o estoque.
        Assert.False(resultado);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void BaixarEstoqueEmLote_DeveReverterBaixasQuandoUmProdutoNaoTiverEstoque() {

        // Cadastra um produto suficiente e outro com estoque insuficiente.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.AddRange(
            new Produto(1, "Teclado", 100m, 10),
            new Produto(2, "Mouse", 50m, 1));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita ambas as baixas, embora o segundo produto não tenha estoque suficiente.
        var resultado = repositorio.BaixarEstoqueEmLote(
            new List<(int ProdutoId, int Quantidade)>
            {
                (1, 2),
                (2, 3)
            });

        // Confirma que a transação desfez a primeira baixa.
        Assert.False(resultado);
        Assert.Equal(10, ObterEstoque(contexto, 1));
        Assert.Equal(1, ObterEstoque(contexto, 2));
    }

    [Fact]
    public void BaixarEstoqueEmLote_DeveSomarQuantidadesDoMesmoProduto() {

        // Cadastra um produto com estoque suficiente para as duas solicitações.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Solicita duas baixas do mesmo produto.
        var resultado = repositorio.BaixarEstoqueEmLote(
            new List<(int ProdutoId, int Quantidade)>
            {
                (1, 2),
                (1, 3)
            });

        // Confirma que as quantidades foram somadas e baixadas corretamente.
        Assert.True(resultado);
        Assert.Equal(5, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void RegistrarEntradaEstoque_DeveAumentarEstoqueQuandoQuantidadeForValida() {

        // Cadastra um produto com estoque inicial conhecido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Registra a entrada de quatro unidades.
        var resultado = repositorio.RegistrarEntradaEstoque(1, 4);

        // Confirma que a entrada foi persistida.
        Assert.Equal(1, resultado);
        Assert.Equal(14, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void RegistrarEntradaEstoque_DeveRetornarZeroQuandoProdutoNaoExistir() {

        // Prepara um banco sem produtos cadastrados.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        var repositorio = new ProdutoRepository(contexto);

        // Tenta registrar entrada para um produto inexistente.
        var resultado = repositorio.RegistrarEntradaEstoque(99, 4);

        // Confirma que nenhuma linha foi alterada.
        Assert.Equal(0, resultado);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public void RegistrarEntradaEstoque_DeveRetornarZeroQuandoQuantidadeForInvalida(int quantidade) {

        // Cadastra um produto com estoque inicial conhecido.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, 10));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Tenta registrar uma entrada com quantidade inválida.
        var resultado = repositorio.RegistrarEntradaEstoque(1, quantidade);

        // Confirma que o estoque não foi alterado.
        Assert.Equal(0, resultado);
        Assert.Equal(10, ObterEstoque(contexto, 1));
    }

    [Fact]
    public void RegistrarEntradaEstoque_DeveRetornarZeroQuandoEstoqueUltrapassarLimiteInteiro() {

        // Cadastra um produto cujo estoque já atingiu o limite de um inteiro.
        using var conexao = CriarConexao();
        using var contexto = CriarContexto(conexao);

        contexto.Produtos.Add(new Produto(1, "Teclado", 100m, int.MaxValue));
        contexto.SaveChanges();

        var repositorio = new ProdutoRepository(contexto);

        // Tenta adicionar uma unidade além do limite permitido.
        var resultado = repositorio.RegistrarEntradaEstoque(1, 1);

        // Confirma que a operação foi recusada sem alterar o estoque.
        Assert.Equal(0, resultado);
        Assert.Equal(int.MaxValue, ObterEstoque(contexto, 1));
    }

    private static SqliteConnection CriarConexao() {

        // Mantém o banco SQLite em memória disponível durante o teste.
        var conexao = new SqliteConnection("Data Source=:memory:");
        conexao.Open();
        return conexao;
    }

    private static AppDbContext CriarContexto(SqliteConnection conexao) {

        // Cria as tabelas do banco em memória.
        var opcoes = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(conexao)
            .Options;

        var contexto = new AppDbContext(opcoes);
        contexto.Database.EnsureCreated();
        return contexto;
    }

    private static int ObterEstoque(AppDbContext contexto, int produtoId) {

        // Consulta o estoque persistido sem reutilizar entidades rastreadas.
        return contexto.Produtos
            .AsNoTracking()
            .Single(p => p.Id == produtoId)
            .Estoque;
    }
}
