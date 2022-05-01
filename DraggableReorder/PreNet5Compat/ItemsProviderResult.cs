using System.Collections.Generic;

namespace DraggableReorder
{
#if !NET5_0_OR_GREATER
    public struct ItemsProviderResult<TItem>
    {
        public IEnumerable<TItem> Items { get; }

        public int TotalItemCount { get; }

        public ItemsProviderResult(IEnumerable<TItem> items, int totalItemCount)
        {
            Items = items;
            TotalItemCount = totalItemCount;
        }
    }
#endif
}
