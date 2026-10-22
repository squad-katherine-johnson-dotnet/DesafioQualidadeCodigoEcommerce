namespace Ecommerce.Domain.Entities;

public class Produto
{
    public int Id { get; private set; }
    public string Nome { get; private set; } = string.Empty;
    public decimal Preco { get; private set; }
    public int Estoque { get; private set; }

    public Produto(int id, string nome, decimal preco, int estoque)
    {
        Id = id;
        AtualizarDados(nome, preco, estoque);
    }

    public void AtualizarDados(string nome, decimal preco, int estoque)
    {
        if (string.IsNullOrWhiteSpace(nome))
            throw new ArgumentException("O nome do produto não pode ser vazio.");

        if (preco <= 0)
            throw new ArgumentException("O preço deve ser maior que zero.");

        if (estoque < 0)
            throw new ArgumentException("O estoque não pode ser negativo.");

        Nome = nome;
        Preco = preco;
        Estoque = estoque;
    }
    public void RegistrarEntradaEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade de entrada deve ser maior que zero.");

        if (quantidade > int.MaxValue - Estoque)
            throw new ArgumentOutOfRangeException(nameof(quantidade), "A quantidade ultrapassa o limite permitido.");

        Estoque += quantidade;
    }

    public void RegistrarSaidaEstoque(int quantidade)
    {
        if (quantidade <= 0)
            throw new ArgumentOutOfRangeException(
                nameof(quantidade), "A quantidade de saída deve ser maior que zero.");

        if (quantidade > Estoque)
            throw new InvalidOperationException("Estoque insuficiente para realizar a saída.");

        Estoque -= quantidade;
    }
}