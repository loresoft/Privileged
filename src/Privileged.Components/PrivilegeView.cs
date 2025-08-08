using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A component that displays different content based on the user's privilege status.
/// </summary>
/// <seealso cref="PrivilegeContext"/>
public class PrivilegeView : ComponentBase
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// </summary>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the content to display if the user is authorized.
    /// </summary>
    [Parameter]
    public RenderFragment<PrivilegeContext>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the content to display if the user is not authorized.
    /// </summary>
    [Parameter]
    public RenderFragment<PrivilegeContext>? Forbidden { get; set; }

    /// <summary>
    /// Gets or sets the content to display if the user is explicitly allowed.
    /// </summary>
    [Parameter]
    public RenderFragment<PrivilegeContext>? Allowed { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize (e.g., "read", "write").
    /// </summary>
    [Parameter, EditorRequired]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the subject to authorize (e.g., a resource or entity).
    /// </summary>
    [Parameter, EditorRequired]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets an optional field to authorize, providing additional scoping for the privilege.
    /// </summary>
    [Parameter]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the specified <see cref="Action"/>, <see cref="Subject"/>, and optional <see cref="Field"/> are allowed.
    /// </summary>
    public bool? IsAllowed { get; set; }

    /// <summary>
    /// Builds the render tree for the component, displaying either the allowed or forbidden content based on the user's privileges.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (IsAllowed == true)
        {
            var authorized = Allowed ?? ChildContent;
            builder.AddContent(0, authorized?.Invoke(PrivilegeContext!));
        }
        else
        {
            builder.AddContent(1, Forbidden?.Invoke(PrivilegeContext!));
        }
    }

    /// <summary>
    /// Called when the component's parameters are set. Validates the parameters and evaluates the user's privileges.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if both <see cref="Allowed"/> and <see cref="ChildContent"/> are specified, or if the <see cref="PrivilegeContext"/> is not provided.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ChildContent != null && Allowed != null)
        {
            throw new InvalidOperationException($"Do not specify both '{nameof(Allowed)}' and '{nameof(ChildContent)}'.");
        }

        if (PrivilegeContext == null)
        {
            throw new InvalidOperationException("PrivilegedView requires a cascading parameter of type PrivilegeContext.");
        }

        IsAllowed = PrivilegeContext?.Allowed(Action, Subject, Field);
    }
}
