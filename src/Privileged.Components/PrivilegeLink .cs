using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Privileged.Components;

/// <summary>
/// A navigation link component that conditionally renders based on user privileges.
/// Extends <see cref="NavLink"/> to provide privilege-based visibility control.
/// </summary>
public class PrivilegeLink : NavLink
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance that contains the rules and logic
    /// for evaluating user privileges. This parameter is required and must be provided
    /// as a cascading parameter from a parent component.
    /// </value>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize for the navigation link.
    /// </summary>
    /// <value>
    /// The action name to check permissions for (e.g., "read", "write", "delete").
    /// Defaults to "read" if not specified.
    /// </value>
    [Parameter]
    public string Action { get; set; } = "read";

    /// <summary>
    /// Gets or sets the subject to authorize for the navigation link.
    /// </summary>
    /// <value>
    /// The subject name representing the resource or entity to check permissions for
    /// (e.g., "Post", "User", "Order"). This parameter is required.
    /// </value>
    [Parameter, EditorRequired]
    public required string Subject { get; set; }

    /// <summary>
    /// Gets or sets an optional qualifier that provides additional scoping for the privilege evaluation.
    /// </summary>
    /// <value>
    /// An optional qualifier string that further scopes the privilege check
    /// (e.g., a field name, category, or specific identifier). Can be <c>null</c>.
    /// </value>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has permission to access this navigation link.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has the required permission for the specified
    /// <see cref="Action"/>, <see cref="Subject"/>, and optional <see cref="Qualifier"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// This property is automatically updated during <see cref="OnParametersSet"/>
    /// based on the current <see cref="PrivilegeContext"/> evaluation.
    /// </remarks>
    protected bool HasPermission { get; set; }

    /// <summary>
    /// Called when the component's parameters are set. Validates the parameters and evaluates the user's privileges.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> cascading parameter is not provided.
    /// </exception>
    /// <remarks>
    /// This method performs privilege evaluation using the <see cref="PrivilegeContext"/>
    /// and updates the <see cref="HasPermission"/> property accordingly.
    /// </remarks>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PrivilegeContext == null)
            throw new InvalidOperationException("Component requires a cascading parameter of type PrivilegeContext.");

        HasPermission = PrivilegeContext.Allowed(Action, Subject, Qualifier);
    }

    /// <summary>
    /// Builds the render tree for the component, conditionally rendering the navigation link based on user privileges.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!HasPermission)
            return;

        base.BuildRenderTree(builder);
    }
}
