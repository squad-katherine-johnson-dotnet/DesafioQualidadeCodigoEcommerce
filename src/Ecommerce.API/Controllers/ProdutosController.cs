using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService)
    {
        _produtoService = produtoService;
    }

    [HttpGet]
    public IActionResult ObterTodos()
    {
        var produtos = _produtoService.ObterTodos();
        return Ok(produtos);
    }

    [HttpGet("{id:int}")]
    public IActionResult ObterPorId(int id)
    {
        var produto = _produtoService.ObterPorId(id);
        if (produto == null)
            return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado." });

        return Ok(produto);
    }

    [HttpPost]
    public IActionResult Criar([FromBody] CriarProdutoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var produtoCriado = _produtoService.Criar(dto);
        return CreatedAtAction(nameof(ObterPorId), new { id = produtoCriado.Id }, produtoCriado);
    }

    [HttpPut("{id:int}")]
    public IActionResult Atualizar(int id, [FromBody] AtualizarProdutoDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var atualizado = _produtoService.Atualizar(id, dto);
        if (!atualizado)
            return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado para atualização." });

        return NoContent();
    }

    [HttpDelete("{id:int}")]
    public IActionResult Remover(int id)
    {
        var removido = _produtoService.Remover(id);
        if (!removido)
            return NotFound(new { mensagem = $"Produto com ID {id} não foi encontrado para remoção." });

        return NoContent();
    }
}