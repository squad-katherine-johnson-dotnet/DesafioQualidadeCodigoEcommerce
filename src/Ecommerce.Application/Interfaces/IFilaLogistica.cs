namespace Ecommerce.Application.Interfaces;

public interface IFilaLogistica {

    ValueTask EnfileirarAsync(int pedidoId, CancellationToken cancellationToken = default);

    ValueTask<int> DesenfileirarAsync(CancellationToken cancellationToken = default);
}

