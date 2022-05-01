using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using DraggableReorder.DefaultItemTemplates;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Components.Web.Virtualization;
#endif

namespace DraggableReorder
{
    public abstract class DraggableItemContainer<TItem, TDraggableItem> : DraggableItemContainer<TItem>
        where TDraggableItem : DraggableItem<TItem>
    {
        protected DraggableItemContainer(string tag) : base(tag)
        {
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if (RefreshException is not null)
            {
                var oldRefreshException = RefreshException;
                RefreshException = null;

                throw oldRefreshException;
            }

            if (_loadedItems is null || ItemTemplate is null)
                return;

            var sequence = 0;

            builder.OpenRegion(sequence);
            builder.OpenElement(++sequence, Tag);
            builder.AddMultipleAttributes(++sequence, OtherAttributes);

            builder.AddEventPreventDefaultAttribute(++sequence, "ondragover", true);
            builder.AddAttribute(++sequence, "ondragover", EventCallback.Factory.Create<DragEventArgs>(this, () => { }));
            builder.AddEventPreventDefaultAttribute(++sequence, "ondragenter", true);
            builder.AddAttribute(++sequence, "ondragenter", EventCallback.Factory.Create<DragEventArgs>(this, () => { }));
            builder.AddEventPreventDefaultAttribute(++sequence, "ondrop", true);
            builder.AddAttribute(++sequence, "ondrop", EventCallback.Factory.Create<DragEventArgs>(this, OnDrop));
            builder.AddAttribute(++sequence, "ondragstart", "event.dataTransfer.setData(\'text\', event.target.id);");
            builder.AddEventStopPropagationAttribute(++sequence, "ondrop", true);
            builder.AddEventStopPropagationAttribute(++sequence, "ondragenter", true);
            builder.AddEventStopPropagationAttribute(++sequence, "ondragend", true);
            builder.AddEventStopPropagationAttribute(++sequence, "ondragover", true);
            builder.AddEventStopPropagationAttribute(++sequence, "ondragleave", true);
            builder.AddEventStopPropagationAttribute(++sequence, "ondragstart", true);
            builder.OpenComponent<CascadingValue<DraggableItemContainer<TItem>>>(++sequence);
            builder.AddAttribute(++sequence, "IsFixed", true);
            builder.AddAttribute(++sequence, "Value", this);
            builder.AddAttribute(++sequence, "ChildContent", (RenderFragment)(builder2 =>
            {
                builder2.OpenComponent<CascadingValue<DraggableService<TItem>>>(++sequence);
                builder2.AddAttribute(++sequence, "IsFixed", true);
                builder2.AddAttribute(++sequence, "Value", DraggableService);
                builder2.AddAttribute(++sequence, "ChildContent", BuildContainerContent(++sequence));
                builder2.CloseComponent();
            }));
            builder.CloseComponent();
            builder.CloseElement();
            builder.CloseRegion();
        }

        protected virtual RenderFragment BuildContainerContent(int sequence) => builder =>
        {
            foreach (var item in _loadedItems!)
            {
                builder.OpenComponent<TDraggableItem>(++sequence);
                builder.AddAttribute(++sequence, "Item", item!);
                builder.AddAttribute(++sequence, "ChildContent",
                    (RenderFragment<TItem>)(item2 =>
                        // ReSharper disable once AccessToModifiedClosure
                        builder2 => builder2.AddContent(++sequence, ItemTemplate!(item2))));
                builder.CloseComponent();
            }
        };
    }


    public abstract class DraggableItemContainer<TItem> : ComponentBase, IDisposable
    {
        protected readonly string Tag;
        private bool _disposedValue;
        private ItemsProviderDelegate<TItem> _itemsProvider = default!;
        internal IList<TItem>? _loadedItems;
        private CancellationTokenSource? _refreshCts;
        protected Exception? RefreshException;

        protected DraggableItemContainer(string tag) => Tag = tag;

        [Inject] internal DraggableService<TItem> DraggableService { get; set; } = default!;

        [Parameter] public IList<TItem>? Items { get; set; }

        [Parameter] public ItemsProviderDelegate<TItem>? ItemsProvider { get; set; }

        [Parameter] public RenderFragment<TItem>? ChildContent { get; set; }

        [Parameter] public RenderFragment<TItem>? ItemTemplate { get; set; }

        [Parameter] public Func<TItem, TItem>? CopyItem { get; set; }

        [Parameter] public EventCallback<TItem> OnReplacedItemDrop { get; set; }

        [Parameter] public EventCallback<TItem> OnItemDrop { get; set; }

        [Parameter] public EventCallback<TItem> OnItemDropRejected { get; set; }

        [Parameter] public Action<TItem>? DragEnd { get; set; }

        [Parameter] public Func<TItem, bool>? AllowsDrag { get; set; }

        [Parameter] public Func<TItem, TItem, bool>? Accepts { get; set; }

        [Parameter(CaptureUnmatchedValues = true)]
        public IDictionary<string, object>? OtherAttributes { get; set; }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        public void OnDragEnd()
        {
            DragEnd?.Invoke(DraggableService.CurrentItem!);

            DraggableService.Reset();
        }

        public void OnDragEnter(TItem item)
        {
            var activeItem = DraggableService.CurrentItem;
            if (item!.Equals(activeItem))
                return;
            if (!IsValidItem())
                return;
            if (!IsItemAccepted(item))
                return;
            DraggableService.TargetItem = item;
            Swap(DraggableService.TargetItem, activeItem!);

            DraggableService.ShouldRender = true;
            StateHasChanged();
            DraggableService.ShouldRender = false;
        }

        public void OnDragLeave()
        {
            DraggableService.TargetItem = default;
            DraggableService.ShouldRender = true;
            StateHasChanged();
            DraggableService.ShouldRender = false;
        }

        public void OnDragStart(TItem item)
        {
            DraggableService.ShouldRender = true;
            DraggableService.CurrentItem = item;
            DraggableService.Items = Items;
            StateHasChanged();
            DraggableService.ShouldRender = false;
        }

        public string CheckIfDragOperationIsInProgress()
        {
            var activeItem = DraggableService.CurrentItem;
            return activeItem == null ? "" : "draggable-in-progress";
        }

        public string CheckIfDraggable(TItem item)
            => AllowsDrag == null ? "" : item == null ? "" : AllowsDrag(item) ? "" : "draggable-no-select";

        public string CheckIfItemIsInTransit(TItem item) => item?.Equals(DraggableService.CurrentItem) == true
            ? "draggable-in-transit no-pointer-events"
            : "";

        public string CheckIfItemIsDragTarget(TItem item)
            => item?.Equals(DraggableService.CurrentItem) == true
                ? ""
                : item?.Equals(DraggableService.TargetItem) == true
                    ? IsItemAccepted(DraggableService.TargetItem!)
                        ? "draggable-dragged-over"
                        : "draggable-dragged-over-denied"
                    : "";

        internal string GetClassesForDraggable()
        {
            var builder = new StringBuilder();
            builder.Append("draggable-draggable");

            return builder.ToString();
        }

        internal string IsItemDraggable(TItem item)
            => AllowsDrag == null ? "true" : item == null ? "false" : AllowsDrag(item).ToString();

        internal void OnDropItemOnSpacing(int newIndex)
        {
            if (_loadedItems is null) return;

            if (!IsDropAllowed())
            {
                DraggableService.Reset();
                return;
            }

            var activeItem = DraggableService.CurrentItem;
            var oldIndex = _loadedItems.IndexOf(activeItem!);
            var sameDropZone = false;
            if (oldIndex == -1)
            {
                if (CopyItem == null) DraggableService.Items!.Remove(activeItem!);
            }
            else
            {
                sameDropZone = true;
                _loadedItems.RemoveAt(oldIndex);
                if (newIndex > oldIndex)
                    newIndex--;
            }

            if (CopyItem == null)
                _loadedItems.Insert(newIndex, activeItem!);
            else
                // for the same zone - do not call CopyItem
                _loadedItems.Insert(newIndex, sameDropZone ? activeItem! : CopyItem(activeItem!));

            //Operation is finished
            DraggableService.Reset();
            OnItemDrop.InvokeAsync(activeItem!);
        }

        protected override async Task OnParametersSetAsync()
        {
            if (ItemsProvider != null)
            {
                if (Items != null)
                {
                    throw new InvalidOperationException(
                        $"Can only use one of either {nameof(ItemsProvider)} or {nameof(Items)} to provider data");
                }

                _itemsProvider = ItemsProvider;
            }
            else
            {
                _itemsProvider = Items is not null
                    ? DefaultItemsProvider
                    : throw new InvalidOperationException(
                        $"{nameof(Items)} must be provided or an {nameof(ItemsProvider)} must be set");
            }

            await RefreshDataCoreAsync(false);

            ItemTemplate ??= ChildContent ?? DefaultItemTemplate;
        }


        protected virtual void OnDrop()
        {
            if (_loadedItems is null)
                return;

            DraggableService.ShouldRender = true;
            if (!IsDropAllowed())
            {
                DraggableService.Reset();
                return;
            }

            var activeItem = DraggableService.CurrentItem;
            if (DraggableService.TargetItem == null)
            {
                if (!_loadedItems.Contains(activeItem!))
                {
                    if (CopyItem == null)
                    {
                        _loadedItems.Insert(_loadedItems.Count, activeItem!);
                        DraggableService.Items!.Remove(activeItem!);
                    }
                    else
                    {
                        _loadedItems.Insert(_loadedItems.Count, CopyItem(activeItem!));
                    }
                }
            }

            DraggableService.Reset();
            StateHasChanged();
            OnItemDrop.InvokeAsync(activeItem!);
        }

        protected override bool ShouldRender() => DraggableService.ShouldRender;

        protected override void OnInitialized()
        {
            DraggableService.StateHasChanged += ForceReRender;

            base.OnInitialized();
        }

        private void Swap(TItem draggedOverItem, TItem activeItem)
        {
            if (_loadedItems is null) return;

            var indexDraggedOverItem = _loadedItems.IndexOf(draggedOverItem);
            var indexCurrentItem = _loadedItems.IndexOf(activeItem);
            if (indexCurrentItem == -1)
            {
                //insert into new zone
                _loadedItems.Insert(indexDraggedOverItem + 1, activeItem);
                //remove from old zone
                DraggableService.Items!.Remove(activeItem);
            }

            if (indexDraggedOverItem == indexCurrentItem)
                return;

            (_loadedItems[indexCurrentItem], _loadedItems[indexDraggedOverItem]) = (_loadedItems[indexDraggedOverItem],
                _loadedItems[indexCurrentItem]);

            OnReplacedItemDrop.InvokeAsync(_loadedItems[indexCurrentItem]);
        }

        private bool IsItemAccepted(TItem dragTargetItem)
            => Accepts == null || Accepts(DraggableService.CurrentItem!, dragTargetItem);

        private bool IsValidItem() => DraggableService.CurrentItem != null;

        internal string GetClassesForSpacing(int spacerId)
        {
            var builder = new StringBuilder();
            builder.Append("draggable-spacing");

            if (DraggableService.ActiveSpacerId == spacerId &&
                _loadedItems!.IndexOf(DraggableService.CurrentItem!) == -1)
            {
                builder.Append(" draggable-spacing-dragged-over");
            }
            else if (DraggableService.ActiveSpacerId == spacerId &&
                     spacerId != _loadedItems!.IndexOf(DraggableService.CurrentItem!) &&
                     spacerId != _loadedItems!.IndexOf(DraggableService.CurrentItem!) + 1)
            {
                builder.Append(" draggable-spacing-dragged-over");
            }

            return builder.ToString();
        }

        private bool IsDropAllowed()
        {
            var activeItem = DraggableService.CurrentItem;
            if (!IsValidItem()) return false;

            if (IsItemAccepted(DraggableService.TargetItem!))
                return true;

            OnItemDropRejected.InvokeAsync(activeItem!);
            return false;
        }

        private async ValueTask RefreshDataCoreAsync(bool renderOnSuccess)
        {
            if (_loadedItems is not null) return;

            _refreshCts?.Cancel();
            CancellationToken cancellationToken;

            if (_itemsProvider == DefaultItemsProvider)
            {
                _refreshCts = null;
                cancellationToken = CancellationToken.None;
            }
            else
            {
                _refreshCts = new CancellationTokenSource();
                cancellationToken = _refreshCts.Token;
            }

            var request = new ItemsProviderRequest(cancellationToken);

            try
            {
                var result = await _itemsProvider(request);

                if (!cancellationToken.IsCancellationRequested)
                {
                    _loadedItems = result.Items.ToList();

                    if (renderOnSuccess) StateHasChanged();
                }
            }
            catch (Exception ex)
            {
                if (ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
                {
                    // do nothing
                }
                else
                {
                    RefreshException = ex;
                    StateHasChanged();
                }
            }
        }

        private void ForceReRender(object? sender, EventArgs? args) => StateHasChanged();

        private RenderFragment DefaultItemTemplate(TItem item) => builder =>
        {
            builder.OpenComponent<DivDraggableItem<TItem>>(0);
            builder.AddAttribute(1, "Item", item!);
            builder.AddAttribute(2, "ChildContent",
                (RenderFragment<TItem>)(_ => builder2 => builder2.AddContent(3, item)));
            builder.CloseComponent();
        };

        private ValueTask<ItemsProviderResult<TItem>> DefaultItemsProvider(ItemsProviderRequest request) =>
#if NET5_0_OR_GREATER
            ValueTask.FromResult(new ItemsProviderResult<TItem>(Items!, Items!.Count));
#else
            new(new ItemsProviderResult<TItem>(Items!, Items!.Count));
#endif

        protected virtual void Dispose(bool disposing)
        {
            if (_disposedValue)
                return;

            if (disposing) DraggableService.StateHasChanged -= ForceReRender;

            _disposedValue = true;
        }
    }

#if !NET5_0_OR_GREATER
    public delegate ValueTask<ItemsProviderResult<TItem>> ItemsProviderDelegate<TItem>(ItemsProviderRequest request);
#else
    public delegate ValueTask<ItemsProviderResult<TItem>> ItemsProviderDelegate<TItem>(DraggableReorder.ItemsProviderRequest request);
#endif
}