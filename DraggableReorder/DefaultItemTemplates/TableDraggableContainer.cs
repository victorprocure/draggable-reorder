using Microsoft.AspNetCore.Components;

namespace DraggableReorder.DefaultItemTemplates
{
    public sealed class TableDraggableContainer<TItem> : DraggableItemContainer<TItem, TrDraggableItem<TItem>>
    {
        [Parameter] public RenderFragment? TableHeader { get; set; }

        public TableDraggableContainer() : base("table")
        {

        }

        protected override RenderFragment BuildContainerContent(int sequence) => builder =>
        {
            if (TableHeader is not null)
            {
                builder.OpenElement(++sequence, "thead");

                builder.AddContent(++sequence, TableHeader);

                builder.CloseElement();
            }

            builder.OpenElement(++sequence, "tbody");
            builder.AddContent(++sequence, base.BuildContainerContent(sequence));
            builder.CloseElement();
        };
    }
}
