
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase
{
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService)
    {
        _estoqueService = estoqueService;
    }

    [HttpGet]
    public IActionResult ObterTodos()
    {
        var produtos = _estoqueService.ObterTodos();
        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        var produto = _estoqueService.ObterPorId(id);

        if (produto == null)
            return NotFound(new
            {
                mensagem = $"Produto com ID {id} não foi encontrado."
            });

        return Ok(produto);
    }

    [HttpGet("abaixo-do-minimo")]
    public IActionResult ObterAbaixoDoMinimo([FromQuery] int quantidadeMinima = 5)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produtos = _estoqueService.ObterAbaixoDoMinimo(
            quantidadeMinima);

        return Ok(produtos);
    }

    [HttpPost("{id:int}/entradas")]
    public IActionResult RegistrarEntrada(int id, [FromBody] MovimentacaoEstoqueDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produto = _estoqueService.RegistrarEntrada(id, dto.Quantidade);

        if (produto == null)
            return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado." });

        return Ok(produto);
    }

    [HttpPost("{id:int}/saidas")]
    public IActionResult RegistrarSaida(int id, [FromBody] MovimentacaoEstoqueDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        try
        {
            var produto = _estoqueService.RegistrarSaida(id, dto.Quantidade);

            if (produto == null)
                return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado." });

            return Ok(produto);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { mensagem = ex.Message });
        }
    }
}