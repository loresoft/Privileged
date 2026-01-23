using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

public class PrivilegeText : ComponentBase
{
    private const string ShowIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z'/><path stroke-linecap='round' stroke-linejoin='round' d='M15 12a3 3 0 11-6 0 3 3 0 016 0z'/></svg>";
    private const string HideIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88'/></svg>";

    private const string ContainerStyle = "display: flex; align-items: center;";
    private const string ButtonStyle = "background: none; border: none; cursor: pointer; padding: 4px; display: flex; align-items: center;";

    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    [Parameter]
    public string? Action { get; set; } = "read";

    [Parameter]
    public string? Subject { get; set; }

    [Parameter]
    public string? Qualifier { get; set; }

    [Parameter]
    public string? Text { get; set; }

    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    [Parameter]
    public string MaskedText { get; set; } = "••••••";

    [Parameter]
    public RenderFragment? MaskedContent { get; set; }


    protected bool? IsAllowed { get; set; }

    protected bool IsShown { get; set; }

    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        IsAllowed = string.IsNullOrWhiteSpace(Subject)
            || PrivilegeContext?.Allowed(Action, Subject, Qualifier) == true;
    }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Container
        builder.OpenElement(0, "div");
        builder.AddAttribute(1, "style", ContainerStyle);

        if (IsAllowed == true)
        {
            builder.OpenElement(2, "button");
            builder.AddAttribute(3, "type", "button");
            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create(this, ToggleVisibility));
            builder.AddAttribute(5, "style", ButtonStyle);
            builder.AddAttribute(6, "aria-label", IsShown ? "Hide" : "Show");
            builder.AddAttribute(7, "title", "Toggle visibility");

            builder.AddMarkupContent(8, IsShown ? HideIcon : ShowIcon);

            builder.CloseElement(); // button
        }

        // Content
        builder.OpenElement(9, "div");

        if (IsShown && IsAllowed == true)
        {
            if (ChildContent != null)
                builder.AddContent(10, ChildContent);
            else
                builder.AddContent(10, Text);
        }
        else
        {
            if (MaskedContent != null)
                builder.AddContent(10, MaskedContent);
            else
                builder.AddContent(10, MaskedText);
        }

        builder.CloseElement(); // div (content)

        builder.CloseElement(); // div (container)
    }

    private void ToggleVisibility()
    {
        IsShown = !IsShown;
    }
}
