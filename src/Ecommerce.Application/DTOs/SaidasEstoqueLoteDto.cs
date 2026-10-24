using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;
public class SaidasEstoqueLoteDto
{
    [Required]
    [MinLength(1)]
    public List<ItemSaidaEstoqueDto> Itens { get; set; } = new();
}