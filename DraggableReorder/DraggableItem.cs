using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace DraggableReorder
{
    public abstract class DraggableItem<TItem> : ComponentBase
    {
        private readonly string _tag;

        [Parameter]
        public TItem Item { get; set; } = default!;

        [Parameter]
        public RenderFragment<TItem>? ChildContent { get; set; }

        [CascadingParameter]
        internal DraggableService<TItem> DraggableService { get; set; } = default!;

        [CascadingParameter]
        internal DraggableItemContainer<TItem> ParentReference { get; set; } = default!;

        protected DraggableItem(string tag)
            => _tag = tag;

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            var currentIndex = ParentReference._loadedItems!.IndexOf(Item);
            var sequence = 0;
            if(currentIndex == 0)
            {
                builder.OpenElement(0, _tag);
                builder.AddAttribute(1, "ondrop", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDropItemOnSpacing(0)));
                builder.AddEventStopPropagationAttribute(2, "ondrop", true);
                builder.AddAttribute(3, "ondragenter", EventCallback.Factory.Create<DragEventArgs>(this, () => DraggableService.ActiveSpacerId = 0));
                builder.AddAttribute(4, "ondragleave", EventCallback.Factory.Create<DragEventArgs>(this, () => DraggableService.ActiveSpacerId = null));
                builder.AddAttribute(5, "class", ParentReference.GetClassesForSpacing(0));
                builder.CloseElement();
                sequence = 6;
            }

            builder.OpenElement(sequence, _tag);
            builder.AddAttribute(sequence + 1, "draggable", ParentReference.IsItemDraggable(Item));
            builder.AddAttribute(sequence + 2, "ondragstart", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDragStart(Item)));
            builder.AddAttribute(sequence + 3, "ondragend", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDragEnd()));
            builder.AddAttribute(sequence + 4, "ondragenter", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDragEnter(Item)));
            builder.AddAttribute(sequence + 5, "ondragleave", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDragLeave()));
            builder.AddAttribute(sequence + 6, "class", $"{ParentReference.GetClassesForDraggable()} {ParentReference.CheckIfItemIsInTransit(Item)} {ParentReference.CheckIfItemIsDragTarget(Item)} {ParentReference.CheckIfDragOperationIsInProgress()} {ParentReference.CheckIfDraggable(Item)}");
            builder.AddContent(sequence + 7, ChildContent?.Invoke(Item));

            builder.CloseElement();

            builder.OpenElement(sequence + 8, _tag);
            builder.AddAttribute(sequence + 9, "ondrop", EventCallback.Factory.Create<DragEventArgs>(this, () => ParentReference.OnDropItemOnSpacing(currentIndex + 1)));
            builder.AddEventStopPropagationAttribute(sequence + 10, "ondrop", true);
            builder.AddAttribute(sequence + 11, "ondragenter", EventCallback.Factory.Create<DragEventArgs>(this, () => DraggableService.ActiveSpacerId = currentIndex + 1));
            builder.AddAttribute(sequence + 12, "ondragleave", EventCallback.Factory.Create<DragEventArgs>(this, () => DraggableService.ActiveSpacerId = null));
            builder.AddAttribute(sequence + 13, "class", $"{ParentReference.CheckIfDragOperationIsInProgress()} {ParentReference.GetClassesForSpacing(currentIndex + 1)}");
            builder.CloseElement();
        }
    }
}
