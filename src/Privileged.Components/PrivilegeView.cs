using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A component that conditionally displays content based on the user's privilege permissions.
/// This component evaluates authorization rules through a cascading <see cref="PrivilegeContext"/>
/// and renders different content templates depending on whether the user is allowed or forbidden
/// to perform the specified action on the given subject and optional field.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeView"/> component provides a declarative way to implement conditional
/// rendering based on user permissions in applications. It supports multiple content
/// templates that are rendered based on the authorization evaluation result.
/// </para>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly.
/// This context contains the privilege rules and performs the actual authorization checks.
/// </para>
/// <para>
/// Content rendering precedence:
/// <list type="number">
/// <item>If the user is allowed: <see cref="Allowed"/> template is rendered if specified, otherwise <see cref="ChildContent"/> is used.</item>
/// <item>If the user is forbidden: <see cref="Forbidden"/> template is rendered if specified.</item>
/// </list>
/// </para>
/// <para>
/// The component supports both single subject authorization via <see cref="Subject"/> and multiple
/// subjects authorization via <see cref="Subjects"/>. When <see cref="Subjects"/> is specified,
/// the component uses the <c>Any</c> extension method to check if the action is allowed on any
/// of the provided subjects.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeContextExtensions"/>
public class PrivilegeView : ComponentBase
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// This parameter is required for the component to function and must be provided
    /// by a parent component or through dependency injection.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance containing privilege rules and authorization logic.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this parameter is <c>null</c> when the component initializes.
    /// </exception>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the content template to display when the user is authorized.
    /// This serves as the default authorized content when <see cref="Allowed"/> is not specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment{T}"/> that receives the current <see cref="PrivilegeContext"/>
    /// as a parameter and returns the content to render when authorization succeeds.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter cannot be used simultaneously with <see cref="Allowed"/>. If both are
    /// specified, an <see cref="InvalidOperationException"/> will be thrown during initialization.
    /// </para>
    /// <para>
    /// The render fragment receives the current <see cref="PrivilegeContext"/> as a parameter,
    /// allowing the content template to access privilege information for additional logic.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment<PrivilegeContext>? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the content template to display when the user is not authorized.
    /// This template is rendered when the privilege evaluation returns <c>false</c>.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment{T}"/> that receives the current <see cref="PrivilegeContext"/>
    /// as a parameter and returns the content to render when authorization fails.
    /// </value>
    /// <remarks>
    /// <para>
    /// This template is optional. If not specified and the user lacks permission,
    /// no content will be rendered for the forbidden state.
    /// </para>
    /// <para>
    /// The render fragment receives the current <see cref="PrivilegeContext"/> as a parameter,
    /// allowing the forbidden content template to access privilege information for conditional logic.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment<PrivilegeContext>? Forbidden { get; set; }

    /// <summary>
    /// Gets or sets the content template to display when the user is explicitly allowed.
    /// This template takes precedence over <see cref="ChildContent"/> when authorization succeeds.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment{T}"/> that receives the current <see cref="PrivilegeContext"/>
    /// as a parameter and returns the content to render when authorization succeeds.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter cannot be used simultaneously with <see cref="ChildContent"/>. If both are
    /// specified, an <see cref="InvalidOperationException"/> will be thrown during initialization.
    /// </para>
    /// <para>
    /// When both <see cref="Allowed"/> and <see cref="ChildContent"/> are available,
    /// <see cref="Allowed"/> takes precedence and will be rendered instead of <see cref="ChildContent"/>.
    /// </para>
    /// <para>
    /// The render fragment receives the current <see cref="PrivilegeContext"/> as a parameter,
    /// allowing the allowed content template to access privilege information for additional logic.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment<PrivilegeContext>? Allowed { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize against the specified subject and optional field.
    /// This parameter is required and defines what operation the user is attempting to perform.
    /// </summary>
    /// <value>
    /// A string representing the action to check permissions for (e.g., "read", "write", "delete", "create").
    /// </value>
    /// <remarks>
    /// <para>
    /// The action must match the actions defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// Actions can be literal values or constants from <see cref="PrivilegeActions"/> if using
    /// predefined action types. Wildcard actions like <see cref="PrivilegeActions.All"/> are supported
    /// if defined in the privilege rules.
    /// </para>
    /// </remarks>
    [Parameter, EditorRequired]
    public required string Action { get; set; }

    /// <summary>
    /// Gets or sets the subject to authorize the action against.
    /// The subject typically represents a resource, entity type, or domain object.
    /// </summary>
    /// <value>
    /// A string representing the subject to check permissions for (e.g., "Post", "User", "Document").
    /// Can be <c>null</c> if <see cref="Subjects"/> is specified instead.
    /// </value>
    /// <remarks>
    /// <para>
    /// The subject value must match the subjects defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// This parameter is mutually exclusive with <see cref="Subjects"/>. If <see cref="Subjects"/>
    /// contains values, this parameter is ignored. Either <see cref="Subject"/> or <see cref="Subjects"/>
    /// should be specified for meaningful authorization checks.
    /// </para>
    /// <para>
    /// Subjects can be literal values or constants from <see cref="PrivilegeSubjects"/> if using
    /// predefined subject types. Wildcard subjects like <see cref="PrivilegeSubjects.All"/> are supported
    /// if defined in the privilege rules.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets a collection of subjects to authorize the action against.
    /// When specified, the component checks if the action is allowed on any of the provided subjects.
    /// </summary>
    /// <value>
    /// An enumerable collection of strings representing multiple subjects to check permissions for.
    /// Can be <c>null</c> or empty if <see cref="Subject"/> is specified instead.
    /// </value>
    /// <remarks>
    /// <para>
    /// When this parameter contains values, the component uses the <see cref="PrivilegeContextExtensions.Any"/>
    /// extension method to determine if the specified <see cref="Action"/> is allowed on at least one
    /// of the provided subjects.
    /// </para>
    /// <para>
    /// This parameter takes precedence over <see cref="Subject"/>. If both are specified,
    /// <see cref="Subjects"/> is used for authorization and <see cref="Subject"/> is ignored.
    /// </para>
    /// <para>
    /// The subjects are checked using short-circuit evaluation - the method returns <c>true</c>
    /// immediately when the first allowed subject is found, making it efficient for checking
    /// permissions across many subjects.
    /// </para>
    /// <para>
    /// Each subject in the collection is case-sensitive and must match subjects defined in the
    /// privilege rules of the <see cref="PrivilegeContext"/>.
    /// </para>
    /// </remarks>
    [Parameter]
    public IEnumerable<string>? Subjects { get; set; }

    /// <summary>
    /// Gets or sets an optional qualifier that provides additional scoping for the privilege check.
    /// This allows for fine-grained permission control at the field or property level.
    /// </summary>
    /// <value>
    /// A string representing the field, property, or qualifier to check permissions for
    /// (e.g., "title", "content", "metadata"). Can be <c>null</c> for general subject-level authorization.
    /// </value>
    /// <remarks>
    /// <para>
    /// The field parameter corresponds to the qualifier parameter in <see cref="PrivilegeContext.Allowed"/>
    /// and allows for granular permission control. For instance, a user might have permission to read
    /// a "Post" but only specific fields like "title" and "summary", not sensitive fields like "metadata".
    /// </para>
    /// <para>
    /// The field value is case-sensitive and must match the qualifiers defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// When <c>null</c>, the authorization check is performed at the subject level without
    /// additional field-specific restrictions.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the specified <see cref="Action"/> is allowed
    /// for the given <see cref="Subject"/> or <see cref="Subjects"/> and optional <see cref="Qualifier"/>.
    /// This property is automatically computed during parameter initialization.
    /// </summary>
    /// <value>
    /// <c>true</c> if the privilege check succeeds and the user is authorized;
    /// <c>false</c> if the privilege check fails and the user is not authorized;
    /// <c>null</c> before the component parameters are initialized.
    /// </value>
    /// <remarks>
    /// <para>
    /// This property is automatically populated in <see cref="OnParametersSet"/> based on the
    /// privilege evaluation results. It reflects the outcome of the authorization check using
    /// the current <see cref="PrivilegeContext"/> and the specified parameters.
    /// </para>
    /// <para>
    /// The value is computed as follows:
    /// <list type="bullet">
    /// <item>If <see cref="Subjects"/> has values: Uses <see cref="PrivilegeContextExtensions.Any"/> to check if any subject allows the action.</item>
    /// <item>Otherwise: Uses <see cref="PrivilegeContext.Allowed"/> with the specified <see cref="Action"/>, <see cref="Subject"/>, and <see cref="Qualifier"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This property can be used by derived components or for debugging purposes to inspect
    /// the authorization result without performing additional privilege checks.
    /// </para>
    /// </remarks>
    public bool? IsAllowed { get; set; }

    /// <summary>
    /// Builds the render tree for the component, displaying either the authorized or forbidden content
    /// based on the result of the privilege evaluation stored in <see cref="IsAllowed"/>.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="RenderTreeBuilder"/> used to construct the component's render tree.
    /// </param>
    /// <remarks>
    /// <para>
    /// The rendering logic follows this precedence:
    /// <list type="number">
    /// <item>If <see cref="IsAllowed"/> is <c>true</c>: Renders <see cref="Allowed"/> if specified, otherwise renders <see cref="ChildContent"/>.</item>
    /// <item>If <see cref="IsAllowed"/> is <c>false</c> or <c>null</c>: Renders <see cref="Forbidden"/> if specified, otherwise renders nothing.</item>
    /// </list>
    /// </para>
    /// <para>
    /// All render fragments receive the current <see cref="PrivilegeContext"/> as a parameter,
    /// enabling the content templates to access privilege information for conditional rendering logic.
    /// </para>
    /// <para>
    /// This method is called automatically by the framework during the component lifecycle
    /// and should not be called directly by application code.
    /// </para>
    /// </remarks>
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
    /// Called when the component's parameters are set or updated. This method validates the component
    /// configuration and performs the privilege evaluation to determine authorization status.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown in the following scenarios:
    /// <list type="bullet">
    /// <item>Both <see cref="Allowed"/> and <see cref="ChildContent"/> are specified simultaneously.</item>
    /// <item>The required <see cref="PrivilegeContext"/> cascading parameter is not provided or is <c>null</c>.</item>
    /// </list>
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// <list type="number">
    /// <item>Validates that <see cref="Allowed"/> and <see cref="ChildContent"/> are not both specified.</item>
    /// <item>Ensures that a valid <see cref="PrivilegeContext"/> is available.</item>
    /// <item>Evaluates user privileges using either <see cref="Subjects"/> (with <see cref="PrivilegeContextExtensions.Any"/>) or single <see cref="Subject"/> authorization.</item>
    /// <item>Sets the <see cref="IsAllowed"/> property based on the evaluation result.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The privilege evaluation logic:
    /// <list type="bullet">
    /// <item>If <see cref="Subjects"/> contains values: Calls <c>PrivilegeContext.Any(Action, Subjects)</c> to check if the action is allowed on any subject.</item>
    /// <item>Otherwise: Calls <c>PrivilegeContext.Allowed(Action, Subject, Field)</c> for single subject authorization.</item>
    /// </list>
    /// </para>
    /// <para>
    /// This method is called automatically by the framework when component parameters change
    /// and should not be called directly by application code.
    /// </para>
    /// </remarks>
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

        if (Subjects?.Any() == true)
        {
            IsAllowed = PrivilegeContext.Any(Action, Subjects);
            return;
        }

        IsAllowed = PrivilegeContext?.Allowed(Action, Subject, Qualifier);
    }
}
