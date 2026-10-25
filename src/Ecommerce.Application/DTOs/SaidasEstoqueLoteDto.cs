using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;
public class RegistrarSaidasEstoqueLoteDto {

    [Required(ErrorMessage = "A lista de itens é obrigatória.")]
    [MinLength(1, ErrorMessage = "Informe pelo menos um item para a saída de estoque.")]
    public List<ItemSaidaEstoqueDto> Itens { get; set; } = new();
}