using System;
using System.Collections.Generic;

namespace DraggableReorder
{
    internal class DraggableService<TItem>
    {
        public TItem? CurrentItem { get; set; }

        public TItem? TargetItem { get; set; }

        public ICollection<TItem>? Items { get; set; }

        public int? ActiveSpacerId { get; set; }

        public void Reset()
        {
            ShouldRender = true;
            CurrentItem = default;
            ActiveSpacerId = null;
            Items = default;
            TargetItem = default;

            StateHasChanged?.Invoke(this, EventArgs.Empty);
        }

        public bool ShouldRender { get; set; } = true;
        public EventHandler? StateHasChanged { get; set; }
    }
}
