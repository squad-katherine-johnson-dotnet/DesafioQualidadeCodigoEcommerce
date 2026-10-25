using System.ComponentModel.DataAnnotations;

namespace Ecommerce.Application.DTOs;

public class CriarPedidoDto {

    [Required(ErrorMessage = "O nome do cliente é obrigatório.")]
    [StringLength(100, ErrorMessage = "O nome do cliente não pode exceder 100 caracteres.")]
    public string ClienteNome { get; set; } = string.Empty;


    [Required(ErrorMessage = "O e-mail do cliente é obrigatório.")]
    [EmailAddress(ErrorMessage = "Informe um e-mail válido.")]
    public string ClienteEmail { get; set; } = string.Empty;


    [Required(ErrorMessage = "A forma de pagamento é obrigatória.")]
    public string FormaPagamento { get; set; } = string.Empty;


    [Required(ErrorMessage = "A lista de itens do pedido é obrigatória.")]
    [MinLength(1, ErrorMessage = "O pedido deve conter pelo menos um item.")]
    public List<CriarItemPedidoDto> Itens { get; set; } = new();
}