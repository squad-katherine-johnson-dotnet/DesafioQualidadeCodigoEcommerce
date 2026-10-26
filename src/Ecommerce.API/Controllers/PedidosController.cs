using Ecommerce.API.Contracts;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase {
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService) {
        _pedidoService = pedidoService;
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

    [HttpPatch("{id:int}/pagamento")]
    public ActionResult<RespostaPadraoDto<PedidoResponseDto>> AtualizarStatusPagamento(
        int id, [FromBody] AtualizarStatusPagamentoDto dto) {

        try {

            var pedido = _pedidoService.AtualizarStatusPagamento(id, dto.Status);

            if (pedido != null) {

                return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                    Sucesso = false,
                    Mensagem = $"Pedido com ID {id} não foi encontrado.",
                    Dados = null
                });
            }

            return Ok(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = true,
                Mensagem = "Status do pagamento atualizado com sucesso!",
                Dados = pedido
            });
        }
        catch (InvalidOperationException ex) {

            return BadRequest(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
    }

    [HttpPatch("{id:int}/logistica")]
    public ActionResult<RespostaPadraoDto<PedidoResponseDto>> AtualizarStatusLogistica(
        int id, [FromBody] AtualizarStatusLogisticaDto dto) {

        try {

            var pedido = _pedidoService.AtualizarStatusLogistica(id, dto.Status, dto.CodigoRastreio);

            if (pedido is null) {

                return NotFound(new RespostaPadraoDto<PedidoResponseDto> {
                    Sucesso = false,
                    Mensagem = $"Pedido com ID {id} não foi encontrado.",
                    Dados = null
                });
            }

            return Ok(new RespostaPadraoDto<PedidoResponseDto> {
                Sucesso = true,
                Mensagem = "Status da logística atualizado com sucesso!",
                Dados = pedido
            });
        }
        catch (InvalidOperationException ex) {

            return BadRequest(new RespostaPadraoDto<PedidoResponseDto> {
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
}