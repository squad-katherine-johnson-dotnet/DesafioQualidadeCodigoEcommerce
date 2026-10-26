using Ecommerce.Domain.Enums;

namespace Ecommerce.Application.DTOs;

public class AtualizarStatusLogisticaDto {
    public StatusLogistica Status { get; set; }
    public string? CodigoRastreio { get; set; }
}

