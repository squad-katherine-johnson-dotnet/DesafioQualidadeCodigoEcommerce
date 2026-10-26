using Ecommerce.API.Contracts;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase {
    private readonly IPedidoService _pedidoService;
    private readonly ILogisticaService _logisticaService;

    public PedidosController(IPedidoService pedidoService, ILogisticaService logisticaService) {

        _pedidoService = pedidoService;
        _logisticaService = logisticaService;
    }

    [HttpGet]
    public ActionResult<RespostaPadraoDto<IEnumerable<PedidoResponseDto>>> ListarPedidos() {

        var pedidos = _pedidoService.ListarPedidos();

        return Ok(new RespostaPadraoDto<IEnumerable<PedidoResponseDto>> {
            Sucesso = true,
            Mensagem = "Pedidos listados com sucesso!",
            Dados = pedidos
        });
    }

    [HttpGet("{id:int}")]
    public ActionResult<RespostaPadraoDto<PedidoResponseDto>> ObterPedidoPorId(int id) {

        var pedido = _pedidoService.ConsultarPedidoPorId(id);

        if (pedido is null) {

            return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = $"Pedido com ID {id} não foi encontrado.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<PedidoResponseDto> {
            Sucesso = true,
            Mensagem = "Pedido encontrado com sucesso!",
            Dados = pedido
        });
    }

    [HttpPost]
    public async Task<ActionResult<RespostaPadraoDto<PedidoResponseDto>>> CriarPedido(CriarPedidoDto dto) {

        try {

            var pedido = await _pedidoService.CriarPedidoAsync(dto);

            var resposta = new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = true,
                Mensagem = "Pedido criado com sucesso!",
                Dados = pedido
            };

            return CreatedAtAction(nameof(ObterPedidoPorId), new { id = pedido.Id }, resposta);
        }
        catch (KeyNotFoundException ex) {

            return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
        catch (InvalidOperationException ex) {

            return Conflict(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
        catch (ArgumentException ex) {

            return BadRequest(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
    }

    [HttpPost("{id:int}/pagamento/reprocessar")]
    public async Task<ActionResult<RespostaPadraoDto<PedidoResponseDto>>> ReprocessarPedido(int id) {

        try {

            var pedido = await _pedidoService.ReprocessarPagamentoAsync(id);

            if (pedido is null) {

                return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                    Sucesso = false,
                    Mensagem = $"Pedido com ID {id} não foi encontrado.",
                    Dados = null
                });
            }

            return Accepted(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = true,
                Mensagem = "Pagamento aceito para reprocessamento.",
                Dados = pedido
            });
        }
        catch (InvalidOperationException ex) {

            return Conflict(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
    }

    [HttpPost("{id:int}/logistica/reenviar")]
    public async Task<ActionResult<RespostaPadraoDto<PedidoResponseDto>>> ReenviarLogistica(int id) {

        try {

            await _logisticaService.ReenviarLogisticaAsync(id);

            return Accepted(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = true,
                Mensagem = "Logística aceita para reenvio.",
                Dados = _pedidoService.ConsultarPedidoPorId(id)
            });
        }
        catch (KeyNotFoundException ex) {

            return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
        catch (InvalidOperationException ex) {

            return Conflict(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
    }

}