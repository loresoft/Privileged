using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A component that renders text content as masked by default, with optional visibility toggle for authorized users.
/// This component evaluates authorization rules through a cascading <see cref="PrivilegeContext"/>
/// and displays content as masked output unless the user has permission and explicitly toggles visibility.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeText"/> component provides a declarative way to implement sensitive
/// data protection with conditional visibility in Blazor applications. It combines privilege-based
/// authorization with user-controlled visibility toggling, making it ideal for displaying sensitive
/// information that should only be accessible to authorized users.
/// </para>
/// <para>
/// <strong>Default behavior:</strong> Content is always rendered as masked initially. If the user has
/// the required permission, a toggle button is displayed allowing them to reveal the actual content.
/// Users without permission see only the masked content with no toggle button available.
/// </para>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly.
/// This context contains the privilege rules and performs the actual authorization checks.
/// </para>
/// <para>
/// Content rendering behavior:
/// <list type="number">
/// <item>If the user is not allowed: Always displays <see cref="MaskedContent"/> if specified, otherwise <see cref="MaskedText"/>. No toggle button is rendered.</item>
/// <item>If the user is allowed but content is hidden (default state): Displays <see cref="MaskedContent"/> if specified, otherwise <see cref="MaskedText"/>. Toggle button is rendered.</item>
/// <item>If the user is allowed and has toggled visibility on: Displays <see cref="ChildContent"/> if specified, otherwise <see cref="Text"/>. Toggle button is rendered.</item>
/// </list>
/// </para>
/// <para>
/// When the user is authorized (has the required privilege), a toggle button is rendered that allows
/// them to show or hide the sensitive content. The button includes eye/eye-slash icons for visual feedback
/// and is fully accessible with appropriate ARIA labels. Without authorization, no toggle button appears.
/// </para>
/// <para>
/// <strong>Special behavior:</strong> When the <see cref="Subject"/> parameter is <c>null</c>, empty,
/// or contains only whitespace, the component assumes all privileges are granted and will render the
/// toggle button as if the user is authorized. This allows for fallback behavior when subject information is not available.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeView"/>
public class PrivilegeText : ComponentBase
{
    private const string ShowIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z'/><path stroke-linecap='round' stroke-linejoin='round' d='M15 12a3 3 0 11-6 0 3 3 0 016 0z'/></svg>";
    private const string HideIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88'/></svg>";

    private const string ContainerStyle = "display: flex; align-items: center;";
    private const string ButtonStyle = "background: none; border: none; cursor: pointer; padding: 4px; display: flex; align-items: center;";

    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// This parameter is required for the component to function and must be provided
    /// by a parent component or through dependency injection.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance containing privilege rules and authorization logic.
    /// </value>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize against the specified subject and optional qualifier.
    /// Defaults to "read" if not specified.
    /// </summary>
    /// <value>
    /// A string representing the action to check permissions for (e.g., "read", "write", "delete", "create").
    /// Defaults to "read".
    /// </value>
    /// <remarks>
    /// <para>
    /// The action must match the actions defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// Actions can be literal values or constants from <see cref="PrivilegeRule"/> if using
    /// predefined action types. Wildcard actions like <see cref="PrivilegeRule.Any"/> are supported
    /// if defined in the privilege rules.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Action { get; set; } = "read";

    /// <summary>
    /// Gets or sets the subject to authorize the action against.
    /// The subject typically represents a resource, entity type, or domain object.
    /// When null, empty, or whitespace, all privileges are assumed to be granted.
    /// </summary>
    /// <value>
    /// A string representing the subject to check permissions for (e.g., "Post", "User", "Document").
    /// Can be <c>null</c> for unrestricted access.
    /// </value>
    /// <remarks>
    /// <para>
    /// The subject value must match the subjects defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// Subjects can be literal values or constants from <see cref="PrivilegeRule"/> if using
    /// predefined subject types. Wildcard subjects like <see cref="PrivilegeRule.Any"/> are supported
    /// if defined in the privilege rules.
    /// </para>
    /// <para>
    /// <strong>Special behavior:</strong> When the Subject parameter is <c>null</c>, empty, or contains only whitespace,
    /// the component assumes all privileges are granted and will render the authorized content with toggle capability
    /// regardless of the privilege context rules. This allows for fallback behavior when subject information is not available.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets an optional qualifier for more granular privilege checking.
    /// The qualifier typically represents a specific field, property, or aspect of the subject.
    /// </summary>
    /// <value>
    /// A string representing the qualifier to check permissions for (e.g., "Email", "Salary", "SSN").
    /// Can be <c>null</c> for subject-level authorization without field-specific checks.
    /// </value>
    /// <remarks>
    /// <para>
    /// The qualifier provides an additional dimension for authorization checks, allowing for
    /// field-level or property-level access control. For example, a user might have permission
    /// to read a User subject but not the SSN qualifier.
    /// </para>
    /// <para>
    /// The qualifier value must match the qualifiers defined in the privilege rules
    /// of the <see cref="PrivilegeContext"/>. The comparison behavior depends on the
    /// <see cref="StringComparer"/> configured in the privilege context.
    /// </para>
    /// <para>
    /// Wildcard qualifiers like <see cref="PrivilegeRule.Any"/> are supported if defined
    /// in the privilege rules, allowing for flexible permission matching.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets or sets the plain text content to display when the user is authorized and has toggled visibility on.
    /// </summary>
    /// <value>
    /// A string containing the sensitive text content to display. Can be <c>null</c> if <see cref="ChildContent"/> is used instead.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter is mutually exclusive with <see cref="ChildContent"/>. If both are specified,
    /// <see cref="ChildContent"/> takes precedence and will be rendered instead of <see cref="Text"/>.
    /// </para>
    /// <para>
    /// Use this parameter for simple text content. For more complex content with markup or other components,
    /// use <see cref="ChildContent"/> instead.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Text { get; set; }

    /// <summary>
    /// Gets or sets the render fragment content to display when the user is authorized and has toggled visibility on.
    /// This parameter takes precedence over <see cref="Text"/> when both are specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that renders the sensitive content to display.
    /// Can be <c>null</c> if <see cref="Text"/> is used instead.
    /// </value>
    /// <remarks>
    /// <para>
    /// When both <see cref="ChildContent"/> and <see cref="Text"/> are specified,
    /// <see cref="ChildContent"/> takes precedence and will be rendered instead of <see cref="Text"/>.
    /// </para>
    /// <para>
    /// Use this parameter for complex content that includes markup, other components, or dynamic rendering logic.
    /// For simple text content, consider using <see cref="Text"/> instead for simplicity.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the text to display when content is masked (hidden or unauthorized).
    /// Defaults to "••••••" (six bullet characters) to indicate hidden content.
    /// </summary>
    /// <value>
    /// A string containing the placeholder text to display when content is masked.
    /// Defaults to "••••••".
    /// </value>
    /// <remarks>
    /// <para>
    /// This text is displayed in two scenarios:
    /// <list type="number">
    /// <item>When the user is authorized but has toggled the visibility off.</item>
    /// <item>When the user is not authorized to view the content.</item>
    /// </list>
    /// </para>
    /// <para>
    /// If <see cref="MaskedContent"/> is specified, it takes precedence over this parameter.
    /// </para>
    /// <para>
    /// Common patterns include bullet characters (••••), asterisks (****), or placeholder text
    /// like "[Hidden]" or "[Redacted]".
    /// </para>
    /// </remarks>
    [Parameter]
    public string MaskedText { get; set; } = "••••••";

    /// <summary>
    /// Gets or sets the render fragment content to display when content is masked (hidden or unauthorized).
    /// This parameter takes precedence over <see cref="MaskedText"/> when both are specified.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that renders the masked placeholder content.
    /// Can be <c>null</c> if <see cref="MaskedText"/> is used instead.
    /// </value>
    /// <remarks>
    /// <para>
    /// This content is displayed in two scenarios:
    /// <list type="number">
    /// <item>When the user is authorized but has toggled the visibility off.</item>
    /// <item>When the user is not authorized to view the content.</item>
    /// </list>
    /// </para>
    /// <para>
    /// When both <see cref="MaskedContent"/> and <see cref="MaskedText"/> are specified,
    /// <see cref="MaskedContent"/> takes precedence and will be rendered instead of <see cref="MaskedText"/>.
    /// </para>
    /// <para>
    /// Use this parameter for complex masked content that includes markup, icons, or other components.
    /// For simple text placeholders, consider using <see cref="MaskedText"/> instead for simplicity.
    /// </para>
    /// </remarks>
    [Parameter]
    public RenderFragment? MaskedContent { get; set; }


    /// <summary>
    /// Gets or sets whether the current user is allowed to view the content based on privilege evaluation.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has permission to view the content and the toggle visibility button should be rendered;
    /// <c>false</c> if the user is not authorized and only masked content should be shown; <c>null</c> if not yet evaluated.
    /// </value>
    /// <remarks>
    /// This property is calculated in <see cref="OnParametersSet"/> based on the <see cref="PrivilegeContext"/>
    /// and the specified <see cref="Action"/>, <see cref="Subject"/>, and <see cref="Qualifier"/>.
    /// When <c>true</c>, the toggle button is rendered to allow the user to show/hide the content.
    /// When <c>false</c>, only the masked content is displayed with no toggle button available.
    /// </remarks>
    protected bool? IsAllowed { get; set; }

    /// <summary>
    /// Gets or sets whether the component has content to display.
    /// </summary>
    /// <value>
    /// <c>true</c> if either <see cref="ChildContent"/> is not null or <see cref="Text"/> is not null/empty/whitespace;
    /// <c>false</c> otherwise.
    /// </value>
    /// <remarks>
    /// This property is calculated in <see cref="OnParametersSet"/> and is used to determine
    /// whether to render the toggle button and masked content.
    /// </remarks>
    protected bool HasContent { get; set; }

    /// <summary>
    /// Gets or sets whether the content is currently shown (unmasked) by user interaction.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has toggled the content to be visible; <c>false</c> if the content is masked (default state).
    /// </value>
    /// <remarks>
    /// <para>
    /// This property tracks the user's toggle state and defaults to <c>false</c>, meaning content is rendered as masked initially.
    /// It is only meaningful when <see cref="IsAllowed"/> is <c>true</c>.
    /// </para>
    /// <para>
    /// When <see cref="IsAllowed"/> is <c>true</c>, users can toggle this state using the eye icon button to reveal or hide the content.
    /// When <see cref="IsAllowed"/> is <c>false</c>, this property has no effect as the content remains masked regardless.
    /// </para>
    /// </remarks>
    protected bool IsShown { get; set; }

    /// <summary>
    /// Evaluates privilege rules when parameters are set or changed.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method is called by the Blazor framework whenever component parameters are set or updated.
    /// It performs authorization evaluation using the <see cref="PrivilegeContext"/> to determine
    /// whether the current user has permission to view the content.
    /// </para>
    /// <para>
    /// The evaluation logic checks if the <see cref="Subject"/> is null, empty, or whitespace (which grants access),
    /// or if the <see cref="PrivilegeContext"/> allows the specified <see cref="Action"/> on the <see cref="Subject"/>
    /// with the optional <see cref="Qualifier"/>.
    /// </para>
    /// </remarks>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        IsAllowed = string.IsNullOrWhiteSpace(Subject)
            || PrivilegeContext?.Allowed(Action, Subject, Qualifier) == true;

        HasContent = ChildContent != null || !string.IsNullOrWhiteSpace(Text);
    }

    /// <summary>
    /// Builds the component's render tree, including the visibility toggle button and content display.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to construct the component's render tree.</param>
    /// <remarks>
    /// <para>
    /// This method constructs a container with the following structure:
    /// <list type="bullet">
    /// <item>A container div with flexbox layout for horizontal alignment.</item>
    /// <item>A toggle button (only rendered when <see cref="IsAllowed"/> is <c>true</c>) with eye/eye-slash icons.</item>
    /// <item>A content div that displays either the actual content or masked content based on <see cref="IsShown"/> and <see cref="IsAllowed"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// Content rendering logic:
    /// <list type="bullet">
    /// <item>If authorized and shown: Renders <see cref="ChildContent"/> or <see cref="Text"/>.</item>
    /// <item>If not authorized or not shown: Renders <see cref="MaskedContent"/> or <see cref="MaskedText"/>.</item>
    /// </list>
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        if (!HasContent)
            return;

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

    /// <summary>
    /// Toggles the visibility state of the content between shown (unmasked) and hidden (masked).
    /// </summary>
    /// <remarks>
    /// This method is invoked when the user clicks the toggle button. It flips the <see cref="IsShown"/>
    /// state, which triggers a re-render to display either the actual content or the masked content.
    /// This method is only accessible when <see cref="IsAllowed"/> is <c>true</c>, as the button
    /// is not rendered for unauthorized users.
    /// </remarks>
    private void ToggleVisibility()
    {
        IsShown = !IsShown;
    }
}
