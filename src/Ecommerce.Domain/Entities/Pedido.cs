namespace Ecommerce.Domain.Entities;

public class Pedido {
    private const decimal LimiteParaDesconto = 500m;
    private const decimal DescontoPorValor = 0.10m;
    private const decimal DescontoVip = 0.20m;

    public int Id { get; private set; }
    public string ClienteNome { get; private set; } = string.Empty;
    public string ClienteEmail { get; private set; } = string.Empty;
    public string FormaPagamento { get; private set; } = string.Empty;
    public DateTime DataPedido { get; private set; }
    public decimal ValorBruto { get; private set; }
    public decimal ValorDesconto { get; private set; }
    public decimal ValorTotal { get; private set; }
    public ICollection<ItemPedido> Itens { get; private set; } = new List<ItemPedido>();

    private Pedido() { }

    public Pedido(
        string clienteNome,
        string clienteEmail,
        string formaPagamento,
        IEnumerable<ItemPedido> itens) {
        if (string.IsNullOrWhiteSpace(clienteNome))
            throw new ArgumentException("O nome do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(clienteEmail))
            throw new ArgumentException("O e-mail do cliente é obrigatório.");

        if (string.IsNullOrWhiteSpace(formaPagamento))
            throw new ArgumentException("A forma de pagamento é obrigatória.");

        ArgumentNullException.ThrowIfNull(itens);

        var listaDeItens = itens.ToList();

        if (listaDeItens.Count == 0)
            throw new ArgumentException("O pedido precisa ter pelo menos um item.");

        ClienteNome = clienteNome;
        ClienteEmail = clienteEmail;
        FormaPagamento = formaPagamento;
        DataPedido = DateTime.UtcNow;
        Itens = listaDeItens;

        CalcularValores();
    }

    private void CalcularValores() {

        ValorBruto = Itens.Sum(item => item.CalcularSubtotal());

        if (ClienteEmail.Contains("vip", StringComparison.OrdinalIgnoreCase)) {

            ValorDesconto = ValorBruto * DescontoVip;
        }
        else if (ValorBruto > LimiteParaDesconto) {

            ValorDesconto = ValorBruto * DescontoPorValor;
        }

        ValorTotal = ValorBruto - ValorDesconto;
    }
}