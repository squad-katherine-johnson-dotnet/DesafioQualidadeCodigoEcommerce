using Ecommerce.Domain.Entities;

namespace Ecommerce.Domain.Interfaces;

public interface IProdutoRepository
{
    IEnumerable<Produto> ObterTodos();
    Produto? ObterPorId(int id);
    Produto Adicionar(Produto produto);
    bool Atualizar(Produto produto);
    bool Remover(int id);
    int BaixarEstoqueSeDisponivel(int produtoId, int quantidade);
    bool BaixarEstoqueEmLote(
        IEnumerable<(int ProdutoId, int Quantidade)> itens);
}