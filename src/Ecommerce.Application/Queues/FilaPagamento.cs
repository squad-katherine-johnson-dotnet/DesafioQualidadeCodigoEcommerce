using Ecommerce.Application.Interfaces;
using System.Threading.Channels;

namespace Ecommerce.Application.Queues {
    public class FilaPagamento : IFilaPagamento {

        private readonly Channel<int> _fila;

        public FilaPagamento() {
            _fila = Channel.CreateUnbounded<int>();
        }

        public ValueTask EnfileirarAsync(int pedidoId, CancellationToken cancellationToken = default) {


            return _fila.Writer.WriteAsync(pedidoId, cancellationToken);
        }

        public ValueTask<int> DesenfileirarAsync(CancellationToken cancellationToken = default) {

            return _fila.Reader.ReadAsync(cancellationToken);
        }
    }
}
