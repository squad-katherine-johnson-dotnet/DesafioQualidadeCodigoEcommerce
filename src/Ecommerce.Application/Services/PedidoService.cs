using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Enums;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class PedidoService : IPedidoService {
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IPedidoTransacaoRepository _pedidoTransacaoRepository;
    private readonly IFilaPagamento _filaPagamento;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        IPedidoTransacaoRepository pedidoTransacaoRepository,
        IFilaPagamento filaPagamento) {

        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _pedidoTransacaoRepository = pedidoTransacaoRepository;
        _filaPagamento = filaPagamento;
    }

    public IEnumerable<PedidoResponseDto> ListarPedidos() {

        return _pedidoRepository.ListarPedidos().Select(Mapear);
    }

    public PedidoResponseDto? ConsultarPedidoPorId(int id) {

        var pedido = _pedidoRepository.ObterPedidoPorId(id);

        return pedido is null ? null : Mapear(pedido);
    }

    public async Task<PedidoResponseDto> CriarPedidoAsync(CriarPedidoDto dto) { 

        var itensPedido = new List<ItemPedido>();

        foreach (var itemDto in dto.Itens) {
            var produto = _produtoRepository.ObterProdutoPorId(itemDto.ProdutoId);

            if (produto is null)
                throw new KeyNotFoundException($"Produto com ID {itemDto.ProdutoId} não foi encontrado.");

            itensPedido.Add(new ItemPedido(produto, itemDto.Quantidade));
        }

        var pedido = new Pedido(
            dto.ClienteNome,
            dto.ClienteEmail,
            dto.FormaPagamento,
            itensPedido);

        var saidas = dto.Itens
            .Select(item => (item.ProdutoId, item.Quantidade))
            .ToList();

        var pedidoCriado = _pedidoTransacaoRepository
            .AdicionarComBaixaDeEstoque(pedido, saidas);

        if (pedidoCriado is null) throw new InvalidOperationException(
                "Não foi possível criar o pedido. Verifique se os produtos existem e possuem estoque suficiente.");

        await _filaPagamento.EnfileirarAsync(pedidoCriado.Id);

        return Mapear(pedidoCriado);
    }

    public PedidoResponseDto? AtualizarStatusPagamento(int pedidoId, StatusPagamento status) {

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) return null;

        pedido.AtualizarStatusPagamento(status);

        _pedidoRepository.AtualizarPedido(pedido);

        return Mapear(pedido);
    }

    public PedidoResponseDto? AtualizarStatusLogistica(int pedidoId, StatusLogistica status, string? codigoRastreio = null) {

        var pedido = _pedidoRepository.ObterPedidoPorId(pedidoId);

        if (pedido is null) return null;

        if (pedido.StatusPagamento != StatusPagamento.Aprovado) throw new InvalidOperationException(
            "A logística só pode ser atualizada após a aprovação do pagamento.");

        pedido.AtualizarStatusLogistica(status, codigoRastreio);

        _pedidoRepository.AtualizarPedido(pedido);

        return Mapear(pedido);
    }

    private static PedidoResponseDto Mapear(Pedido pedido) {
        return new PedidoResponseDto {
            Id = pedido.Id,
            ClienteNome = pedido.ClienteNome,
            ClienteEmail = pedido.ClienteEmail,
            DataPedido = pedido.DataPedido,
            FormaPagamento = pedido.FormaPagamento,
            ValorBruto = pedido.ValorBruto,
            ValorDesconto = pedido.ValorDesconto,
            ValorTotal = pedido.ValorTotal,
            StatusPagamento = pedido.StatusPagamento,
            StatusLogistica = pedido.StatusLogistica,
            CodigoRastreio = pedido.CodigoRastreio,
            Itens = pedido.Itens.Select(item => new ItemPedidoResponseDto {
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Subtotal = item.CalcularSubtotal()
            }).ToList()
        };
    }
}