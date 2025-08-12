using System.Collections.Generic;

namespace Blazor.GoldenLayout.Models
{
    public abstract record Node(string Id)
    {
        public double SizeFraction { get; init; } = 1;
    }

    public sealed record Row(string Id, List<Node> Children) : Node(Id);
    public sealed record Column(string Id, List<Node> Children) : Node(Id);
    public sealed record Stack(string Id, List<ComponentRef> Tabs, int ActiveIndex = 0) : Node(Id);

    public sealed record ComponentRef(string ComponentId, string? Title = null);

    public static class NodeBuilders
    {
        public static Row Row(params Node[] children) => new("row-" + System.Guid.NewGuid().ToString("N"), new List<Node>(children));
        public static Column Column(params Node[] children) => new("col-" + System.Guid.NewGuid().ToString("N"), new List<Node>(children));
        public static Stack Stack(params ComponentRef[] tabs) => new("stack-" + System.Guid.NewGuid().ToString("N"), new List<ComponentRef>(tabs));
        public static ComponentRef ComponentRef(string id, string? title = null) => new(id, title);
    }
}
