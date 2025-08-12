using System;

namespace Blazor.GoldenLayout.Services
{
    public enum DockEdge { Center, Top, Right, Bottom, Left }

    public sealed class DragService
    {
        public record DragState(string ComponentId, string FromStackId);
        public DragState? Current { get; private set; }
        public event Action? Changed;
        public (string stackId, DockEdge edge)? Hover { get; private set; }

        public void Begin(string componentId, string fromStack)
        { Current = new DragState(componentId, fromStack); Changed?.Invoke(); }

        public void UpdateHover(string stackId, DockEdge edge)
        { Hover = (stackId, edge); Changed?.Invoke(); }

        public (string stackId, DockEdge edge)? ConsumeHover() => Hover;

        public void End() { Current = null; Hover = null; Changed?.Invoke(); }
    }
}
