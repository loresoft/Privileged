using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Routing;

namespace Privileged.Components;

/// <summary>
/// A navigation link component that conditionally renders based on user privileges.
/// Extends <see cref="NavLink"/> to provide privilege-based visibility control.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeLink"/> component extends the standard <see cref="NavLink"/> component
/// to automatically hide navigation links when users lack the required permissions. This provides a
/// seamless way to build privilege-aware navigation menus and UI elements that only display
/// authorized actions to users.
/// </para>
/// <para>
/// The component evaluates permissions using a cascading <see cref="PrivilegeContext"/> parameter
/// and renders the navigation link only when the user has the required privileges. When permissions
/// are denied, the component renders nothing, effectively hiding the link from unauthorized users.
/// </para>
/// <para>
/// This component supports all standard <see cref="NavLink"/> features including CSS class management,
/// active link styling, and navigation matching behavior, while adding privilege-based conditional rendering.
/// </para>
/// </remarks>
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
    /// <remarks>
    /// This cascading parameter is typically provided by a parent component such as
    /// <c>PrivilegeContextView</c> or directly through the application's root layout.
    /// The component will throw an <see cref="InvalidOperationException"/> if this
    /// parameter is not available.
    /// </remarks>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize for the navigation link.
    /// </summary>
    /// <value>
    /// The action name to check permissions for (e.g., "read", "write", "delete").
    /// Defaults to "read" if not specified.
    /// </value>
    /// <remarks>
    /// Actions typically represent verbs describing what operation the user wants to perform.
    /// Common actions include "read", "create", "update", "delete", or custom business-specific
    /// actions like "publish", "approve", or "archive". The action is evaluated against the
    /// current user's privileges to determine if the navigation link should be displayed.
    /// </remarks>
    [Parameter]
    public required string Action { get; set; } = "read";

    /// <summary>
    /// Gets or sets the subject to authorize for the navigation link.
    /// </summary>
    /// <value>
    /// The subject name representing the resource or entity to check permissions for
    /// (e.g., "Post", "User", "Order"). This parameter is mutually exclusive with <see cref="Subjects"/>.
    /// </value>
    /// <remarks>
    /// The subject typically represents a business entity, resource type, or domain object
    /// that the user wants to perform an action on. When both <see cref="Subject"/> and
    /// <see cref="Subjects"/> are provided, <see cref="Subjects"/> takes precedence and
    /// this parameter is ignored.
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the collection of subjects to authorize for the navigation link.
    /// </summary>
    /// <value>
    /// A collection of subject names to check permissions for. When provided, the component
    /// will render the link if the user has the required <see cref="Action"/> permission
    /// for ANY of the specified subjects. This parameter takes precedence over <see cref="Subject"/>.
    /// </value>
    /// <remarks>
    /// This parameter is useful when a navigation link should be visible if the user has
    /// permission to perform the action on any one of multiple subjects. For example,
    /// a "Content Management" link might be shown if the user can edit either "Post" or "Article" subjects.
    /// The evaluation uses short-circuit logic, stopping at the first allowed subject.
    /// </remarks>
    [Parameter]
    public IEnumerable<string>? Subjects { get; set; }

    /// <summary>
    /// Gets or sets an optional qualifier that provides additional scoping for the privilege evaluation.
    /// </summary>
    /// <value>
    /// An optional qualifier string that further scopes the privilege check
    /// (e.g., a field name, category, or specific identifier). Can be <c>null</c>.
    /// </value>
    /// <remarks>
    /// Qualifiers provide fine-grained permission control by adding an additional dimension
    /// to privilege evaluation. Common use cases include field-level permissions (e.g., "title", "content"),
    /// category-based access (e.g., "public", "private"), or tenant-specific scoping.
    /// When specified, all three components (action, subject, qualifier) must match for permission to be granted.
    /// </remarks>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has permission to access this navigation link.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has the required permission for the specified
    /// <see cref="Action"/>, <see cref="Subject"/> or <see cref="Subjects"/>, and optional <see cref="Qualifier"/>;
    /// otherwise, <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is automatically computed during <see cref="OnParametersSet"/>
    /// based on the current <see cref="PrivilegeContext"/> evaluation. The computation logic is:
    /// </para>
    /// <list type="bullet">
    /// <item><description>If <see cref="Subjects"/> is provided and contains values, evaluates using <see cref="PrivilegeContextExtensions.Any(PrivilegeContext, string, IEnumerable{string})"/></description></item>
    /// <item><description>Otherwise, evaluates using <see cref="PrivilegeContext.Allowed(string?, string?, string?)"/> with <see cref="Subject"/> and <see cref="Qualifier"/></description></item>
    /// </list>
    /// <para>
    /// The permission check is re-evaluated each time the component parameters are updated,
    /// ensuring the visibility state remains current with any changes to the privilege context or parameters.
    /// </para>
    /// </remarks>
    protected bool HasPermission { get; set; }

    /// <summary>
    /// Called when the component's parameters are set. Validates the parameters and evaluates the user's privileges.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> cascading parameter is not provided.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Calls the base <see cref="NavLink.OnParametersSet"/> method to ensure proper inheritance behavior</description></item>
    /// <item><description>Validates that the required <see cref="PrivilegeContext"/> cascading parameter is available</description></item>
    /// <item><description>Evaluates user privileges based on the current parameter values</description></item>
    /// <item><description>Updates the <see cref="HasPermission"/> property with the evaluation result</description></item>
    /// </list>
    /// <para>
    /// The privilege evaluation follows this precedence:
    /// If <see cref="Subjects"/> contains any values, it uses <c>PrivilegeContext.Any(Action, Subjects)</c>
    /// to check if the user has permission for the action on any of the specified subjects.
    /// Otherwise, it uses <c>PrivilegeContext.Allowed(Action, Subject, Qualifier)</c> for single-subject evaluation.
    /// </para>
    /// </remarks>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PrivilegeContext == null)
            throw new InvalidOperationException("Component requires a cascading parameter of type PrivilegeContext.");

        if (Subjects?.Any() == true)
        {
            HasPermission = PrivilegeContext.Any(Action, Subjects);
            return;
        }

        HasPermission = PrivilegeContext.Allowed(Action, Subject, Qualifier);
    }

    /// <summary>
    /// Builds the render tree for the component, conditionally rendering the navigation link based on user privileges.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    /// <remarks>
    /// <para>
    /// This method implements the privilege-based conditional rendering logic. When <see cref="HasPermission"/>
    /// is <c>false</c>, the method returns immediately without adding any content to the render tree,
    /// effectively hiding the navigation link from unauthorized users.
    /// </para>
    /// <para>
    /// When permissions are granted (<see cref="HasPermission"/> is <c>true</c>), the method delegates
    /// to the base <see cref="NavLink.BuildRenderTree"/> method to render the standard navigation link
    /// with all inherited functionality including active CSS class management, href handling, and click behavior.
    /// </para>
    /// <para>
    /// This approach ensures that unauthorized navigation links are completely absent from the DOM,
    /// providing both security and a clean user interface that only shows available actions.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!HasPermission)
            return;

        base.BuildRenderTree(builder);
    }
}
