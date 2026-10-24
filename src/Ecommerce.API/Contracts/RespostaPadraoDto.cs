namespace Ecommerce.API.Contracts;

public class RespostaPadraoDto<T> {
    public bool Sucesso { get; set; }
    public string Mensagem { get; set; } = string.Empty;
    public T? Dados { get; set; }
}

