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
            return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado." });

        return Ok(produto);
    }

    [HttpGet("abaixo-do-minimo")]
    public IActionResult ObterAbaixoDoMinimo(
    [FromQuery] int quantidadeMinima = 5)
    {
        if (quantidadeMinima < 0)
            return BadRequest(new { mensagem = "A quantidade mínima não pode ser negativa." });

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

    [HttpPost("saidas-em-lote")]
    public IActionResult RegistrarSaidasEmLote([FromBody] SaidasEstoqueLoteDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var itens = dto.Itens
            .Select(item => (ProdutoId: item.ProdutoId, Quantidade: item.Quantidade))
            .ToList();

        var sucesso = _estoqueService.RegistrarSaidasEmLote(itens);

        if (!sucesso)
        {
            return Conflict(new { mensagem = "Não foi possível realizar todas as baixas. Verifique os produtos e os estoques disponíveis." });
        }

        return Ok(new { mensagem = "Todas as baixas de estoque foram realizadas com sucesso." });
    }
}