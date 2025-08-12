using Microsoft.AspNetCore.Components;

namespace Blazor.GoldenLayout
{
    public class GlComponentBase : ComponentBase
    {
        [CascadingParameter] public GlLayoutBase Root { get; set; } = default!;
        [Parameter] public string Id { get; set; } = default!;
        [Parameter] public string? Title { get; set; }
        [Parameter] public RenderFragment? ChildContent { get; set; }

        protected override void OnInitialized()
            => Root.Register(Id, ChildContent ?? (b => { }));
    }
}
