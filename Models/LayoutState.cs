using System;
using System.Collections.Generic;
using System.Text.Json;

namespace Blazor.GoldenLayout.Models
{
    public sealed class LayoutState
    {
        public Node Root { get; set; }
        public Dictionary<string, string> Titles { get; } = new();
        public string LayoutId { get; set; } = Guid.NewGuid().ToString("N");
        public Dictionary<string, PopoutMeta> Popped { get; set; } = new();
        public Dictionary<string, DockMemory> LastDock { get; set; } = new();

        public LayoutState(Node root) => Root = root;

        public string Serialize() => JsonSerializer.Serialize(this);
        public static LayoutState Deserialize(string json)
            => JsonSerializer.Deserialize<LayoutState>(json)!;
    }

    public sealed class PopoutMeta
    {
        public string WindowId { get; set; } = string.Empty;
        public DateTimeOffset Since { get; set; } = DateTimeOffset.UtcNow;
    }

    public sealed class DockMemory
    {
        public string StackId { get; set; } = string.Empty;
        public int Index { get; set; }
        public DateTimeOffset Updated { get; set; } = DateTimeOffset.UtcNow;
    }
}
