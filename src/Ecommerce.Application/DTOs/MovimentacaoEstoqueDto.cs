using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;

public class MovimentacaoEstoqueDto
{
    [Range(1, int.MaxValue, ErrorMessage = "A quantidade deve ser maior que zero.")]
    public int Quantidade { get; set; }
}