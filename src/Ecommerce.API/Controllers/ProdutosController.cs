using Ecommerce.Application.DTOs;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase
{
    [HttpGet]
    public IActionResult ObterTodos()
    {
        return Ok(new List<ProdutoResponseDto>());
    }

    [HttpPost]
    public IActionResult Criar([FromBody] CriarProdutoDto dto)
    {
        var produtoCriado = new ProdutoResponseDto
        {
            Id = 1,
            Nome = dto.Nome,
            Preco = dto.Preco,
            Estoque = dto.Estoque
        };

        return CreatedAtAction(nameof(ObterTodos), new { id = produtoCriado.Id }, produtoCriado);
    }
}