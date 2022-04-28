using System.Threading;

namespace DraggableReorder
{
    public struct ItemsProviderRequest
    {
        public CancellationToken CancellationToken { get; }

        public ItemsProviderRequest(CancellationToken cancellationToken) => CancellationToken = cancellationToken;
    }
}
