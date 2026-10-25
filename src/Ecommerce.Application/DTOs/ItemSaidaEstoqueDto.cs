using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;
public class ItemSaidaEstoqueDto {

    [Range(1, int.MaxValue, ErrorMessage = "O ID do produto deve ser maior que zero.")]
    public int ProdutoId { get; set; }


    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}