using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// Displays differing content depending on the user's privilege status.
/// </summary>
/// <seealso cref="Privileged.PrivilegeContext"/>
public class PrivilegedView : ComponentBase
{

    /// <summary>
    /// Gets or sets the privilege context.
    /// </summary>
    /// <value>
    /// The privilege context.
    /// </value>
    [CascadingParameter] protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is authorized.
    /// </summary>
    [Parameter] public RenderFragment<PrivilegeContext>? ChildContent { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is not authorized.
    /// </summary>
    [Parameter] public RenderFragment<PrivilegeContext>? Forbidden { get; set; }

    /// <summary>
    /// The content that will be displayed if the user is authorized.
    /// If you specify a value for this parameter, do not also specify a value for <see cref="ChildContent"/>.
    /// </summary>
    [Parameter] public RenderFragment<PrivilegeContext>? Authorized { get; set; }

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

    /// <summary>
    /// Gets or sets weather the specified <see cref="Action"/>, <see cref="Subject"/> and optional <see cref="Field"/> is authorized.
    /// </summary>
    /// <value>
    /// The weather the specified <see cref="Action"/>, <see cref="Subject"/> and optional <see cref="Field"/> is authorized.
    /// </value>
    public bool? IsAuthorized { get; set; }

    /// <inheritdoc />
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (IsAuthorized == true)
        {
            var authorized = Authorized ?? ChildContent;
            builder.AddContent(0, authorized?.Invoke(PrivilegeContext!));
        }
        else
        {
            builder.AddContent(0, Forbidden?.Invoke(PrivilegeContext!));
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

        if (PrivilegeContext == null)
        {
            throw new InvalidOperationException($"PrivilegedView requires a cascading parameter of type PrivilegeContext.");
        }

        IsAuthorized = PrivilegeContext?.Authorized(Action, Subject, Field);
    }
}
