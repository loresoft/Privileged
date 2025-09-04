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
/// are denied, the component can either hide the link entirely or render it as a disabled span element,
/// depending on the <see cref="HideForbidden"/> parameter.
/// </para>
/// <para>
/// This component supports all standard <see cref="NavLink"/> features including CSS class management,
/// active link styling, and navigation matching behavior, while adding privilege-based conditional rendering.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;!-- Basic privilege link --&gt;
/// &lt;PrivilegeLink Action="read" Subject="Post" href="/posts"&gt;
///     View Posts
/// &lt;/PrivilegeLink&gt;
/// 
/// &lt;!-- Link with multiple subjects (shown if user can edit any of these) --&gt;
/// &lt;PrivilegeLink Action="edit" 
///               Subjects='new[] { "Post", "Article", "News" }' 
///               href="/content"&gt;
///     Content Management
/// &lt;/PrivilegeLink&gt;
/// 
/// &lt;!-- Link with qualifier for fine-grained permissions --&gt;
/// &lt;PrivilegeLink Action="update" 
///               Subject="User" 
///               Qualifier="Profile"
///               href="/profile"&gt;
///     Edit Profile
/// &lt;/PrivilegeLink&gt;
/// 
/// &lt;!-- Hidden when forbidden instead of disabled --&gt;
/// &lt;PrivilegeLink Action="admin" 
///               Subject="System" 
///               HideForbidden="true"
///               href="/admin"&gt;
///     Admin Panel
/// &lt;/PrivilegeLink&gt;
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="NavLink"/>
/// <seealso cref="PrivilegeButton"/>
public class PrivilegeLink : NavLink
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// This parameter is required for the component to function properly.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance that contains the rules and logic
    /// for evaluating user privileges. This parameter is required and must be provided
    /// as a cascading parameter from a parent component.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown during <see cref="OnParametersSet"/> if this parameter is not available when the component initializes.
    /// </exception>
    /// <remarks>
    /// This cascading parameter is typically provided by a parent component such as
    /// <see cref="PrivilegeContextView"/> or directly through the application's root layout.
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
    /// This parameter is required and must be specified.
    /// </value>
    /// <remarks>
    /// <para>
    /// Actions typically represent verbs describing what operation the user wants to perform.
    /// Common actions include "read", "create", "update", "delete", or custom business-specific
    /// actions like "publish", "approve", or "archive". The action is evaluated against the
    /// current user's privileges to determine if the navigation link should be displayed.
    /// </para>
    /// <para>
    /// This parameter is required for privilege evaluation. If not specified, the component
    /// may not function as expected.
    /// </para>
    /// </remarks>
    [Parameter]
    public required string Action { get; set; } = "read";

    /// <summary>
    /// Gets or sets the subject to authorize for the navigation link.
    /// When null, empty, or whitespace, all privileges are assumed to be granted.
    /// </summary>
    /// <value>
    /// The subject name representing the resource or entity to check permissions for
    /// (e.g., "Post", "User", "Order"). This parameter is mutually exclusive with <see cref="Subjects"/>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The subject typically represents a business entity, resource type, or domain object
    /// that the user wants to perform an action on. When both <see cref="Subject"/> and
    /// <see cref="Subjects"/> are provided, <see cref="Subjects"/> takes precedence and
    /// this parameter is ignored.
    /// </para>
    /// <para>
    /// <strong>Special behavior:</strong> When the Subject parameter is <c>null</c>, empty, or contains only whitespace,
    /// the component assumes all privileges are granted and will render the navigation link regardless
    /// of the privilege context rules. This allows for fallback behavior when subject information is not available.
    /// </para>
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
    /// <para>
    /// This parameter is useful when a navigation link should be visible if the user has
    /// permission to perform the action on any one of multiple subjects. For example,
    /// a "Content Management" link might be shown if the user can edit either "Post" or "Article" subjects.
    /// The evaluation uses short-circuit logic, stopping at the first allowed subject.
    /// </para>
    /// <para>
    /// When this parameter contains values, it takes precedence over the <see cref="Subject"/> parameter,
    /// and the single <see cref="Subject"/> value is ignored.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;PrivilegeLink Action="edit" 
    ///               Subjects='new[] { "Post", "Article", "News" }' 
    ///               href="/content"&gt;
    ///     Content Management
    /// &lt;/PrivilegeLink&gt;
    /// </code>
    /// </example>
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
    /// <para>
    /// Qualifiers provide fine-grained permission control by adding an additional dimension
    /// to privilege evaluation. Common use cases include field-level permissions (e.g., "title", "content"),
    /// category-based access (e.g., "public", "private"), or tenant-specific scoping.
    /// When specified, all three components (action, subject, qualifier) must match for permission to be granted.
    /// </para>
    /// <para>
    /// This parameter is optional and when not specified, the privilege check applies to the entire subject
    /// without additional scoping restrictions.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether links should be hidden when the user
    /// lacks the required permissions instead of being displayed in a disabled state.
    /// </summary>
    /// <value>
    /// <c>true</c> to hide the link when permission is denied; <c>false</c> to show
    /// the link as a disabled span element. Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter controls the visibility behavior when the user lacks the required permissions:
    /// </para>
    /// <list type="bullet">
    /// <item><description><c>false</c> (default): The link is rendered as a disabled span element with the same content and styling, providing visual feedback that the action exists but is not available</description></item>
    /// <item><description><c>true</c>: The link is not rendered at all, hiding the action from users who cannot access it</description></item>
    /// </list>
    /// <para>
    /// The choice between these approaches depends on your application's security and UX requirements.
    /// Hiding forbidden links provides a cleaner interface but may make it harder for users to understand
    /// what actions might be available with different permissions.
    /// </para>
    /// </remarks>
    [Parameter]
    public bool HideForbidden { get; set; }

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
    /// <item><description>If <see cref="Subject"/> is null, empty, or whitespace, permission is granted (fallback behavior)</description></item>
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
    /// </para>
    /// <list type="number">
    /// <item><description>If <see cref="Subjects"/> contains any values, it uses <c>PrivilegeContext.Any(Action, Subjects)</c> to check if the user has permission for the action on any of the specified subjects.</description></item>
    /// <item><description>If <see cref="Subject"/> is null, empty, or whitespace, permission is granted (fallback behavior).</description></item>
    /// <item><description>Otherwise, it uses <c>PrivilegeContext.Allowed(Action, Subject, Qualifier)</c> for single-subject evaluation.</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> cascading parameter is not provided.
    /// </exception>
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

        HasPermission = string.IsNullOrWhiteSpace(Subject)
            || PrivilegeContext.Allowed(Action, Subject, Qualifier);
    }

    /// <summary>
    /// Builds the render tree for the component, conditionally rendering the navigation link based on user privileges.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    /// <remarks>
    /// <para>
    /// This method implements the privilege-based conditional rendering logic with the following behavior:
    /// </para>
    /// <list type="number">
    /// <item><description><strong>Hidden when forbidden:</strong> If <see cref="HideForbidden"/> is <c>true</c> and <see cref="HasPermission"/> is <c>false</c>, returns immediately without adding any content to the render tree.</description></item>
    /// <item><description><strong>Normal navigation link:</strong> If <see cref="HasPermission"/> is <c>true</c>, delegates to the base <see cref="NavLink.BuildRenderTree"/> method to render the standard navigation link.</description></item>
    /// <item><description><strong>Disabled span element:</strong> If <see cref="HasPermission"/> is <c>false</c> and <see cref="HideForbidden"/> is <c>false</c>, renders a span element with the same attributes and content but without navigation functionality.</description></item>
    /// </list>
    /// <para>
    /// When rendering a disabled span (no permission but not hidden), the component:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Creates a span element instead of an anchor</description></item>
    /// <item><description>Preserves all additional attributes from <see cref="NavLink.AdditionalAttributes"/></description></item>
    /// <item><description>Renders the same child content as the normal link would have</description></item>
    /// </list>
    /// <para>
    /// This approach ensures that unauthorized navigation links either completely absent from the DOM (when hidden)
    /// or visually present but non-functional (when disabled), providing both security and appropriate user feedback.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Do not render if the user does not have permission and HideForbidden is true
        if (HideForbidden && !HasPermission)
            return;

        // render normally if we have permission
        if (HasPermission)
        {
            base.BuildRenderTree(builder);
            return;
        }

        // render a span with the same attributes if no permission
        builder.OpenElement(0, "span");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "class", CssClass);
        builder.AddContent(3, ChildContent);
        builder.CloseElement();
    }
}
