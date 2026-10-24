using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Ecommerce.Domain.Entities;
using Ecommerce.Domain.Interfaces;

namespace Ecommerce.Application.Services;

public class PedidoService : IPedidoService
{
    private readonly IPedidoRepository _pedidoRepository;
    private readonly IProdutoRepository _produtoRepository;
    private readonly IEstoqueService _estoqueService;

    public PedidoService(
        IPedidoRepository pedidoRepository,
        IProdutoRepository produtoRepository,
        IEstoqueService estoqueService)
    {
        _pedidoRepository = pedidoRepository;
        _produtoRepository = produtoRepository;
        _estoqueService = estoqueService;
    }

    public IEnumerable<PedidoResponseDto> ListarPedidos()
    {
        return _pedidoRepository.ObterTodos().Select(Mapear);
    }

    public PedidoResponseDto? ConsultarPedidoPorId(int id)
    {
        var pedido = _pedidoRepository.ObterPorId(id);

        return pedido is null ? null : Mapear(pedido);
    }

    public PedidoResponseDto CriarPedido(CriarPedidoDto dto)
    {
        var itensPedido = new List<ItemPedido>();

        foreach (var itemDto in dto.Itens)
        {
            var produto = _produtoRepository.ObterPorId(itemDto.ProdutoId);

            if (produto is null)
                throw new KeyNotFoundException(
                    $"Produto com ID {itemDto.ProdutoId} não foi encontrado.");

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

        if (!_estoqueService.RegistrarSaidasDeEstoqueEmLote(saidas))
            throw new InvalidOperationException(
                "Não foi possível realizar a baixa do estoque.");

        var pedidoCriado = _pedidoRepository.Adicionar(pedido);

        return Mapear(pedidoCriado);
    }

    private static PedidoResponseDto Mapear(Pedido pedido)
    {
        return new PedidoResponseDto
        {
            Id = pedido.Id,
            ClienteNome = pedido.ClienteNome,
            ClienteEmail = pedido.ClienteEmail,
            DataPedido = pedido.DataPedido,
            FormaPagamento = pedido.FormaPagamento,
            ValorBruto = pedido.ValorBruto,
            ValorDesconto = pedido.ValorDesconto,
            ValorTotal = pedido.ValorTotal,
            Itens = pedido.Itens.Select(item => new ItemPedidoResponseDto
            {
                ProdutoId = item.ProdutoId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                Subtotal = item.CalcularSubtotal()
            }).ToList()
        };
    }
}