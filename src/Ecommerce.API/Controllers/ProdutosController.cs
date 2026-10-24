using Ecommerce.API.Contracts;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProdutosController : ControllerBase {
    private readonly IProdutoService _produtoService;

    public ProdutosController(IProdutoService produtoService) {
        _produtoService = produtoService;
    }

    [HttpGet]
    public ActionResult<RespostaPadraoDto<IEnumerable<ProdutoResponseDto>>> ListarProdutos() {

        var produtos = _produtoService.ObterTodos();

        return Ok(new RespostaPadraoDto<IEnumerable<ProdutoResponseDto>> {
            Sucesso = true,
            Mensagem = "Produtos listados com sucesso!",
            Dados = produtos
        });
    }

    [HttpGet("{id:int}")]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> ObterProdutoPorId(int id) {

        var produto = _produtoService.ObterPorId(id);

        if (produto == null) {

            return NotFound(new RespostaPadraoDto<ProdutoResponseDto> {
                Sucesso = false,
                Mensagem = $"Produto com ID {id} não foi encontrado.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<ProdutoResponseDto> {
            Sucesso = true,
            Mensagem = "Produto encontrado com sucesso!",
            Dados = produto
        });
    }

    [HttpPost]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> CadastrarProduto([FromBody] CriarProdutoDto dto) {

        var produtoCriado = _produtoService.Criar(dto);

        var resposta = new RespostaPadraoDto<ProdutoResponseDto> {
            Sucesso = true,
            Mensagem = "Produto cadastrado com sucesso!",
            Dados = produtoCriado
        };

        return CreatedAtAction(nameof(ObterProdutoPorId), new { id = produtoCriado.Id }, resposta);
    }

    [HttpPut("{id:int}")]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> AtualizarProduto(int id, [FromBody] AtualizarProdutoDto dto) {

        var atualizado = _produtoService.Atualizar(id, dto);

        if (!atualizado) {

            return NotFound(new RespostaPadraoDto<ProdutoResponseDto> {
                Sucesso = false,
                Mensagem = $"Produto com ID {id} não foi encontrado para atualização.",
                Dados = null
            });
        }

        var produtoAtualizado = _produtoService.ObterPorId(id);

        return Ok(new RespostaPadraoDto<ProdutoResponseDto> {
            Sucesso = true,
            Mensagem = "Produto atualizado com sucesso!",
            Dados = produtoAtualizado
        });
    }

    [HttpDelete("{id:int}")]
    public ActionResult<RespostaPadraoDto<object>> RemoverProduto(int id) {

        var removido = _produtoService.Remover(id);

        if (!removido) {

            return NotFound(new RespostaPadraoDto<object> {
                Sucesso = false,
                Mensagem = $"Produto com ID {id} não foi encontrado para remoção.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<object> {
            Sucesso = true,
            Mensagem = "Produto removido com sucesso!",
            Dados = null
        });
    }
}