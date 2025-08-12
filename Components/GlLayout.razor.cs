using System;
using System.Linq;
using System.Collections.Generic;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;
using Blazor.GoldenLayout.Models;

namespace Blazor.GoldenLayout
{
    public class GlLayoutBase : ComponentBase
    {
        [Parameter] public LayoutState Layout { get; set; } = default!;
        [Parameter] public RenderFragment? ChildContent { get; set; }
        [Parameter] public EventCallback<LayoutState> OnLayoutChanged { get; set; }

        private readonly Dictionary<string, RenderFragment> _components = new();

        internal void Register(string id, RenderFragment content) => _components[id] = content;
        internal RenderFragment ResolveComponent(string id) => _components.TryGetValue(id, out var rf) ? rf : Missing(id);
        protected RenderFragment ResolveComponent(ComponentRef cref) => ResolveComponent(cref.ComponentId);

        private RenderFragment Missing(string id) => b =>
        {
            b.OpenElement(0, "div");
            b.AddAttribute(1, "class", "gl-missing");
            b.AddContent(2, $"No component registered for Id '{id}'.");
            b.CloseElement();
        };

        protected void Resize(List<Node> siblings, int leftIndex, double delta)
        {
            var left = siblings[leftIndex];
            var right = siblings[leftIndex + 1];
            var total = left.SizeFraction + right.SizeFraction;
            var newLeft = Math.Clamp(left.SizeFraction + delta, 0.1, total - 0.1);
            var newRight = total - newLeft;
            siblings[leftIndex] = left with { SizeFraction = newLeft };
            siblings[leftIndex + 1] = right with { SizeFraction = newRight };
            InvokeAsync(StateHasChanged);
            OnLayoutChanged.InvokeAsync(Layout);
        }

        protected void OnStackChanged(Stack _) => OnLayoutChanged.InvokeAsync(Layout);
        protected void OnPointerUp(PointerEventArgs _)
        {
            // Drop handler can be wired from GlLayout.razor via services
            OnLayoutChanged.InvokeAsync(Layout);
        }

        public Task NotifyChanged() => OnLayoutChanged.InvokeAsync(Layout);

        // ----- Drag-dock utilities -----
        public enum DockEdge { Center, Top, Right, Bottom, Left }

        protected void ApplyDock(string componentId, string fromStack, string toStack, DockEdge edge)
        {
            // 1) remove from source
            var (src, srcParent) = FindStackWithParent(Layout.Root, fromStack);
            var tab = src.Tabs.First(t => t.ComponentId == componentId);
            src.Tabs.Remove(tab);

            // 2) destination
            var (dst, dstParent) = FindStackWithParent(Layout.Root, toStack);

            if (edge == DockEdge.Center)
            {
                dst.Tabs.Add(tab);
                dst.ActiveIndex = dst.Tabs.Count - 1;
                Remember(tab.ComponentId, dst.Id, dst.ActiveIndex);
            }
            else if (edge is DockEdge.Left or DockEdge.Right)
            {
                var row = dstParent as Row ?? new Row(Guid.NewGuid().ToString("N"), new() { dst });
                var newStack = new Stack(Guid.NewGuid().ToString("N"), new() { tab });
                var insertAt = IndexOfChild(dstParent, dst) + (edge == DockEdge.Right ? 1 : 0);

                if (dstParent is Row existingRow)
                {
                    existingRow.Children.Insert(insertAt, newStack);
                }
                else
                {
                    ReplaceChild(dstParent, dst, row);
                    row.Children.Insert(insertAt, newStack);
                }
                Remember(tab.ComponentId, newStack.Id, 0);
            }
            else // Top/Bottom
            {
                var col = dstParent as Column ?? new Column(Guid.NewGuid().ToString("N"), new() { dst });
                var newStack = new Stack(Guid.NewGuid().ToString("N"), new() { tab });
                var insertAt = IndexOfChild(dstParent, dst) + (edge == DockEdge.Bottom ? 1 : 0);

                if (dstParent is Column existingCol)
                {
                    existingCol.Children.Insert(insertAt, newStack);
                }
                else
                {
                    ReplaceChild(dstParent, dst, col);
                    col.Children.Insert(insertAt, newStack);
                }
                Remember(tab.ComponentId, newStack.Id, 0);
            }

            Layout.Root = LayoutNormalizer.Normalize(Layout.Root);
        }

        protected void Remember(string componentId, string stackId, int index)
        {
            Layout.LastDock[componentId] = new DockMemory { StackId = stackId, Index = index, Updated = DateTimeOffset.UtcNow };
        }

        protected (Stack stack, Node? parent) FindStackWithParent(Node node, string id)
        {
            switch (node)
            {
                case Stack s when s.Id == id:
                    return (s, null);
                case Row r:
                    foreach (var ch in r.Children)
                    {
                        var res = FindStackWithParent(ch, id);
                        if (res.stack is not null) return (res.stack, r);
                    }
                    break;
                case Column c:
                    foreach (var ch in c.Children)
                    {
                        var res = FindStackWithParent(ch, id);
                        if (res.stack is not null) return (res.stack, c);
                    }
                    break;
            }
            throw new InvalidOperationException($"Stack {id} not found");
        }

        protected int IndexOfChild(Node? parent, Node child)
        {
            if (parent is Row r) return r.Children.IndexOf(child);
            if (parent is Column c) return c.Children.IndexOf(child);
            return -1;
        }

        protected void ReplaceChild(Node? parent, Node oldChild, Node newParent)
        {
            if (parent is Row r)
            {
                var idx = r.Children.IndexOf(oldChild); r.Children[idx] = newParent;
            }
            else if (parent is Column c)
            {
                var idx = c.Children.IndexOf(oldChild); c.Children[idx] = newParent;
            }
        }
    }
}
