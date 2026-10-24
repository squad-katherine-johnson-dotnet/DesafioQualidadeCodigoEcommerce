using Ecommerce.API.Contracts;
using Ecommerce.Application.DTOs;
using Ecommerce.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace Ecommerce.API.Controllers;

[ApiController]
[Route("api/[controller]")]
public class EstoqueController : ControllerBase {
    private readonly IEstoqueService _estoqueService;

    public EstoqueController(IEstoqueService estoqueService) {
        _estoqueService = estoqueService;
    }

    [HttpGet]
    public ActionResult<RespostaPadraoDto<IEnumerable<ProdutoResponseDto>>> ConsultarEstoque() {

        var produtos = _estoqueService.ObterTodos();

        return Ok(new RespostaPadraoDto<IEnumerable<ProdutoResponseDto>> {
            Sucesso = true,
            Mensagem = "Estoque listado com sucesso!",
            Dados = produtos
        });
    }

    [HttpGet("{id:int}")]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> ConsultarEstoquePorProdutoId(int id) {

        var produto = _estoqueService.ObterPorId(id);

        if (produto == null) {

            return NotFound(new RespostaPadraoDto<ProdutoResponseDto> {
                Sucesso = false,
                Mensagem = $"Produto com ID {id} não foi encontrado.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<ProdutoResponseDto> {
            Sucesso = true,
            Mensagem = "Estoque encontrado com sucesso!",
            Dados = produto
        });
    }

    [HttpGet("abaixo-do-minimo")]
    public ActionResult<RespostaPadraoDto<IEnumerable<ProdutoResponseDto>>> ListarProdutosAbaixoDoEstoqueMinimo(
        [FromQuery] int quantidadeMinima = 5) {

        if (quantidadeMinima < 0) {

            return BadRequest(new RespostaPadraoDto<IEnumerable<ProdutoResponseDto>> {
                Sucesso = false,
                Mensagem = "A quantidade mínima não pode ser negativa.",
                Dados = null
            });
        }

        var produtos = _estoqueService.ObterAbaixoDoMinimo(quantidadeMinima);

        return Ok(new RespostaPadraoDto<IEnumerable<ProdutoResponseDto>> {
            Sucesso = true,
            Mensagem = "Consulta de produtos abaixo do estoque mínimo realizada com sucesso.",
            Dados = produtos
        });
    }

    [HttpPost("{id:int}/entradas")]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> RegistrarEntradaDeEstoque(
        int id, [FromBody] MovimentacaoEstoqueDto dto) {

        var produto = _estoqueService.RegistrarEntrada(id, dto.Quantidade);

        if (produto == null) {

            return NotFound(new RespostaPadraoDto<ProdutoResponseDto> {
                Sucesso = false,
                Mensagem = $"Produto com ID {id} não foi encontrado.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<ProdutoResponseDto> {
            Sucesso = true,
            Mensagem = $"Entrada de estoque do produto com ID {id} registrada com sucesso!",
            Dados = produto
        });
    }

    [HttpPost("{id:int}/saidas")]
    public ActionResult<RespostaPadraoDto<ProdutoResponseDto>> RegistrarSaidaDeEstoque(
        int id, [FromBody] MovimentacaoEstoqueDto dto) {

        try {

            var produto = _estoqueService.RegistrarSaida(id, dto.Quantidade);

            if (produto == null) {

                return NotFound(new RespostaPadraoDto<ProdutoResponseDto> {
                    Sucesso = false,
                    Mensagem = $"Produto com ID {id} não foi encontrado.",
                    Dados = null
                });
            }

            return Ok(new RespostaPadraoDto<ProdutoResponseDto> {
                Sucesso = true,
                Mensagem = $"Saída de estoque do produto com ID {id} registrada com sucesso!",
                Dados = produto
            });

        }
        catch (InvalidOperationException ex) {

            return Conflict(new RespostaPadraoDto<ProdutoResponseDto> { 
                Sucesso = false,
                Mensagem = ex.Message,
                Dados = null
            });
        }
    }

    [HttpPost("saidas-em-lote")]
    public ActionResult<RespostaPadraoDto<object>> RegistrarSaidasDeEstoqueEmLote(
        [FromBody] SaidasEstoqueLoteDto dto) {

        var itens = dto.Itens
            .Select(item => (ProdutoId: item.ProdutoId, Quantidade: item.Quantidade))
            .ToList();

        var sucesso = _estoqueService.RegistrarSaidasEmLote(itens);

        if (!sucesso) {

            return Conflict(new RespostaPadraoDto<object> { 
                Sucesso = false,
                Mensagem = "Não foi possível realizar todas as baixas. Verifique os produtos e os estoques disponíveis.",
                Dados = null
            });
        }

        return Ok(new RespostaPadraoDto<object> {
            Sucesso = true,
            Mensagem = "Todas as baixas de estoque foram realizadas com sucesso!",
            Dados = null
        });
    }
}