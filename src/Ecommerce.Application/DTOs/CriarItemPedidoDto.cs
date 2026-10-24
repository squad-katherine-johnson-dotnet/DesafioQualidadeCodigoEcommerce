using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;

public class CriarItemPedidoDto
{
    [Range(1, int.MaxValue)]
    public int ProdutoId { get; set; }

    [Range(1, int.MaxValue)]
    public int Quantidade { get; set; }
}