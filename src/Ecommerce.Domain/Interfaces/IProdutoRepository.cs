using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IProdutoRepository {
    IEnumerable<Produto> ListarProdutos();
    Produto? ObterProdutoPorId(int id);
    Produto AdicionarProduto(Produto produto);
    bool AtualizarProduto(Produto produto);
    bool RemoverProduto(int id);
    int BaixarEstoqueSeDisponivel(int produtoId, int quantidade);
    bool BaixarEstoqueEmLote(IEnumerable<(int ProdutoId, int Quantidade)> itens);
}