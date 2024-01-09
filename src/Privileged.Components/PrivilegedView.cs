using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// Displays differing content depending on the user's Privileged authorization status.
/// </summary>
/// <seealso cref="Privileged.AuthorizationContext"/>
public class PrivilegedView : ComponentBase
{

    [CascadingParameter] private AuthorizationContext? AuthorizationContext { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is authorized.
    /// </summary>
    [Parameter] public RenderFragment<AuthorizationContext>? ChildContent { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is not authorized.
    /// </summary>
    [Parameter] public RenderFragment<AuthorizationContext>? Forbidden { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is authorized.
    /// If you specify a value for this parameter, do not also specify a value for <see cref="ChildContent"/>.
    /// </summary>
    [Parameter] public RenderFragment<AuthorizationContext>? Authorized { get; set; }

    /// <summary>
    /// The action to authorize.
    /// </summary>
    [Parameter, EditorRequired] public string? Action { get; set; }

    /// <summary>
    /// The subject to authorize.
    /// </summary>
    [Parameter, EditorRequired] public string? Subject { get; set; }

    /// <summary>
    /// The optional field to authorize.
    /// </summary>
    [Parameter] public string? Field { get; set; }

    public bool? IsAuthorized { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (IsAuthorized == true)
        {
            var authorized = Authorized ?? ChildContent;
            builder.AddContent(0, authorized?.Invoke(AuthorizationContext!));
        }
        else
        {
            builder.AddContent(0, Forbidden?.Invoke(AuthorizationContext!));
        }
    }

    /// <inheritdoc />
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (ChildContent != null && Authorized != null)
        {
            throw new InvalidOperationException($"Do not specify both '{nameof(Authorized)}' and '{nameof(ChildContent)}'.");
        }

        if (AuthorizationContext == null)
        {
            throw new InvalidOperationException($"PrivilegedView requires a cascading parameter of type AuthorizationContext.");
        }

        IsAuthorized = AuthorizationContext?.Authorized(Action, Subject, Field);
    }
}
