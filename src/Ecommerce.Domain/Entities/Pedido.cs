using Ecommerce.Domain.Enums;

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
    public StatusPagamento StatusPagamento { get; private set; } = StatusPagamento.Pendente;
    public StatusLogistica StatusLogistica { get; private set; } = StatusLogistica.Pendente;
    public string? CodigoRastreio { get; private set; }

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

        CalcularValorTotal();
    }

    public void AtualizarStatusPagamento(StatusPagamento status) {

        if (StatusPagamento == StatusPagamento.Aprovado) throw new InvalidOperationException(
            "Um pagamento aprovado não pode ser reprocessado.");

        if (status == StatusPagamento.Pendente && StatusPagamento != StatusPagamento.Recusado)
            throw new InvalidOperationException("Somente um pagament recusado pode voltar para pendente.");

        if (status == StatusPagamento.Recusado && StatusPagamento != StatusPagamento.Pendente)
            throw new InvalidOperationException("Somente um pagamento pendente pode ser recusado.");

        StatusPagamento = status;
    }

    public void AtualizarStatusLogistica(StatusLogistica status, string? codigoRastreio = null) {

        if (StatusLogistica == StatusLogistica.Enviado) throw new InvalidOperationException(
            "Uma logística já enviada não pode ser alterada.");

        if (status == StatusLogistica.Enviado) {

            if (StatusPagamento != StatusPagamento.Aprovado) throw new InvalidOperationException(
                "A logística só pode ser enviada após a aprovação do pagamento.");

            if (string.IsNullOrWhiteSpace(codigoRastreio)) throw new ArgumentException(
                "O código de rastreio é obrigatório para o envio.");
        }

        if (status == StatusLogistica.Pendente && StatusLogistica != StatusLogistica.Falhou)
            throw new InvalidOperationException("Somente uma logística que falhou pode voltar para pendente.");

        StatusLogistica = status;

        if (status == StatusLogistica.Enviado) {
            CodigoRastreio = codigoRastreio;
        }
        else {
            CodigoRastreio = null;
        }
    }

    private void CalcularValorTotal() {

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