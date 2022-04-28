using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.Threading;
using System.Threading.Tasks;
#if NET5_0_OR_GREATER
using Microsoft.AspNetCore.Components.Web.Virtualization;
#endif
namespace DraggableReorder
{
    public class Draggable<TItem> : ComponentBase
    {
        private ItemsProviderDelegate<TItem> _itemsProvider = default!;
        private RenderFragment<TItem>? _itemTemplate;
        private CancellationTokenSource? _refreshCts;
        private IEnumerable<TItem>? _loadedItems;
        private Exception? _refreshException;

        [Parameter]
        public RenderFragment<TItem>? ChildContent { get; set; }

        [Parameter]
        public RenderFragment<TItem>? ItemContent { get; set; }

        [Parameter]
        public ICollection<TItem>? Items { get; set; }

        [Parameter]
        public ItemsProviderDelegate<TItem>? ItemsProvider { get; set; }

        [Parameter]
        public float ItemSize { get; set; } = 50f;

        public async Task RefreshDataAsync()
        {
            await RefreshDataCoreAsync(renderOnSuccess: false);
        }

        protected override void OnParametersSet()
        {
            if (ItemsProvider != null)
            {
                if (Items != null)
                {
                    throw new InvalidOperationException($"Can only use one of either {nameof(ItemsProvider)} or {nameof(Items)} to provider data");
                }

                _itemsProvider = ItemsProvider;
            }
            else if (Items != null)
            {
                _itemsProvider = DefaultItemsProvider;

                var refreshTask = RefreshDataCoreAsync(renderOnSuccess: false);

                Debug.Assert(refreshTask.IsCompletedSuccessfully);
            }
            else
            {
                throw new InvalidOperationException($"{nameof(Items)} must be provided or an {nameof(ItemsProvider)} must be set");
            }

            _itemTemplate = ItemContent ?? ChildContent;
        }

        protected override void BuildRenderTree(RenderTreeBuilder builder)
        {
            if(_refreshException is not null)
            {
                var oldRefreshException = _refreshException;
                _refreshException = null;

                throw oldRefreshException;
            }

            if (_loadedItems is not null && _itemTemplate is not null)
            {
                var items = _loadedItems;
                builder.OpenRegion(0);

                foreach (var item in items)
                {
                    _itemTemplate!(item)(builder);
                }

                builder.CloseRegion();
            }
        }

        private async ValueTask RefreshDataCoreAsync(bool renderOnSuccess)
        {
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
                    _loadedItems = result.Items;

                    if (renderOnSuccess)
                    {
                        StateHasChanged();
                    }
                }
            }catch(Exception ex)
            {
                if(ex is OperationCanceledException oce && oce.CancellationToken == cancellationToken)
                {
                    // do nothing
                }
                else
                {
                    _refreshException = ex;
                    StateHasChanged();
                }
            }
        }

        private ValueTask<ItemsProviderResult<TItem>> DefaultItemsProvider(ItemsProviderRequest request)
        {
#if NET5_0_OR_GREATER
            return ValueTask.FromResult(new ItemsProviderResult<TItem>(Items!, Items!.Count));
#else
            return new ValueTask<ItemsProviderResult<TItem>>(new ItemsProviderResult<TItem>(Items!, Items!.Count));
#endif
        }
    }

#if !NET5_0_OR_GREATER
    public delegate ValueTask<ItemsProviderResult<TItem>> ItemsProviderDelegate<TItem>(ItemsProviderRequest request);
#else
    public delegate ValueTask<ItemsProviderResult<TItem>> ItemsProviderDelegate<TItem>(DraggableReorder.ItemsProviderRequest request);
#endif
}
