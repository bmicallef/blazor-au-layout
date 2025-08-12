using System;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace Blazor.GoldenLayout.Services
{
    public sealed partial class PopoutService : IAsyncDisposable
    {
        private readonly IJSRuntime _js;
        private IJSObjectReference? _mod;
        private DotNetObjectReference<PopoutService>? _selfRef;
        public event Action<string>? Closed;
        public event Action<string, string>? ChannelMessage; // (type,json)
        public string LayoutId { get; private set; } = Guid.NewGuid().ToString("N");

        public PopoutService(IJSRuntime js) => _js = js;

        public async Task InitializeAsync()
        {
            if (_mod is not null) return;
            _mod = await _js.InvokeAsync<IJSObjectReference>("import", "./gl.popout.js");
            _selfRef = DotNetObjectReference.Create(this);
            await _mod.InvokeVoidAsync("initChannel", LayoutId);
            await _mod.InvokeVoidAsync("onClosed", _selfRef);
            await _mod.InvokeVoidAsync("onMessage", _selfRef);
        }

        public async Task<string?> OpenAsync(string url, string? name = null, string? features = null)
            => await _mod!.InvokeAsync<string?>("openPopout", url, name, features);

        public async Task CloseAsync(string id)
            => await _mod!.InvokeVoidAsync("closePopout", id);

        public Task BroadcastAsync(object message)
            => _mod!.InvokeVoidAsync("post", message).AsTask();

        [JSInvokable] public void OnPopoutClosed(string id) => Closed?.Invoke(id);

        [JSInvokable]
        public void OnChannelMessage(string json)
        {
            using var doc = System.Text.Json.JsonDocument.Parse(json);
            var t = doc.RootElement.GetProperty("t").GetString() ?? string.Empty;
            ChannelMessage?.Invoke(t, json);
        }

        public async ValueTask DisposeAsync()
        {
            _selfRef?.Dispose();
            if (_mod is not null) await _mod.DisposeAsync();
        }
    }
}
