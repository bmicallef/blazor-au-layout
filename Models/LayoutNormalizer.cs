using System;
using System.Linq;
using System.Collections.Generic;

namespace Blazor.GoldenLayout.Models
{
    public static class LayoutNormalizer
    {
        public static Node Normalize(Node root)
        {
            return NormalizeInternal(root) ?? Placeholder();
        }

        private static Node? NormalizeInternal(Node node)
        {
            switch (node)
            {
                case Stack s:
                    return s.Tabs.Count == 0 ? null : Clamp(s);
                case Row r:
                    for (int i = 0; i < r.Children.Count; i++)
                    {
                        var norm = NormalizeInternal(r.Children[i]);
                        if (norm is null) { r.Children.RemoveAt(i); i--; }
                        else r.Children[i] = norm;
                    }
                    return Collapse(r);
                case Column c:
                    for (int i = 0; i < c.Children.Count; i++)
                    {
                        var norm = NormalizeInternal(c.Children[i]);
                        if (norm is null) { c.Children.RemoveAt(i); i--; }
                        else c.Children[i] = norm;
                    }
                    return Collapse(c);
            }
            return node;
        }

        private static Node? Collapse(Row r)
        {
            if (r.Children.Count == 0) return null;
            if (r.Children.Count == 1) return r.Children[0];
            return r; // keep as Row
        }

        private static Node? Collapse(Column c)
        {
            if (c.Children.Count == 0) return null;
            if (c.Children.Count == 1) return c.Children[0];
            return c; // keep as Column
        }

        private static Stack Placeholder() => new(Guid.NewGuid().ToString("N"), new());

        private static Stack Clamp(Stack s)
        {
            s.ActiveIndex = Math.Clamp(s.ActiveIndex, 0, Math.Max(0, s.Tabs.Count - 1));
            return s;
        }
    }
}
