using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PedidosController : ControllerBase
{
    private readonly IPedidoService _pedidoService;

    public PedidosController(IPedidoService pedidoService)
    {
        _pedidoService = pedidoService;
    }

    [HttpGet]
    public ActionResult<IEnumerable<PedidoResponseDto>> ObterTodos()
    {
        return Ok(_pedidoService.ObterTodos());
    }

    [HttpGet("{id:int}")]
    public ActionResult<PedidoResponseDto> ObterPorId(int id)
    {
        var pedido = _pedidoService.ObterPorId(id);

        if (pedido is null)
            return NotFound(new { mensagem = $"Pedido com ID {id} não foi encontrado." });

        return Ok(pedido);
    }

    [HttpPost]
    public ActionResult<PedidoResponseDto> Criar(CriarPedidoDto dto)
    {
        try
        {
            var pedido = _pedidoService.Criar(dto);

            return CreatedAtAction(
                nameof(ObterPorId),
                new { id = pedido.Id },
                pedido);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { mensagem = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
        catch (ArgumentException ex)
        {
            return BadRequest(new { mensagem = ex.Message });
        }
    }
}