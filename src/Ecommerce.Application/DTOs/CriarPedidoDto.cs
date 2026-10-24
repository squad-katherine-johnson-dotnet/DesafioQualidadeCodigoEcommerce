using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;

public class CriarPedidoDto
{
    [Required]
    [StringLength(100)]
    public string ClienteNome { get; set; } = string.Empty;

    [Required]
    [EmailAddress]
    public string ClienteEmail { get; set; } = string.Empty;

    [Required]
    public string FormaPagamento { get; set; } = string.Empty;

    [Required]
    [MinLength(1)]
    public List<CriarItemPedidoDto> Itens { get; set; } = new();
}