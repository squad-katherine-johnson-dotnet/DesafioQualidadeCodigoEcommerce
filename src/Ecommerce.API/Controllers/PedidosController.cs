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
    public ActionResult<RespostaPadraoDto<PedidoResponseDto>> CriarPedido(CriarPedidoDto dto) {

        try {

            var pedido = _pedidoService.CriarPedido(dto);

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
}