using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.AspNetCore.Components.Web;

namespace Privileged.Components;

/// <summary>
/// A button component that integrates privilege-based access control and busy state management.
/// This component automatically manages authorization checks, busy states during async operations,
/// and provides declarative permission-based rendering through a cascading <see cref="PrivilegeContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeButton"/> component provides a comprehensive solution for secure user interactions
/// by combining authorization checks with modern user experience patterns such as busy indicators and
/// disabled states during async operations.
/// </para>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly.
/// This context contains the privilege rules and performs the actual authorization checks that determine
/// whether the button should be enabled, disabled, or hidden from the user interface.
/// </para>
/// <para>
/// Button state precedence:
/// <list type="number">
/// <item>Hidden: If <see cref="HideForbidden"/> is <c>true</c> and the user lacks permission.</item>
/// <item>Disabled: If <see cref="Disabled"/> is <c>true</c>, the button is busy, or the user lacks permission.</item>
/// <item>Busy: When <see cref="Busy"/> is <c>true</c> or the <see cref="Trigger"/> callback is executing.</item>
/// <item>Normal: When the user has permission and the button is not disabled or busy.</item>
/// </list>
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeView"/>
public class PrivilegeButton : ComponentBase
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// This parameter is required for the component to function and must be provided
    /// by a parent component or through dependency injection.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance that contains the rules and logic
    /// for evaluating user privileges. This parameter is required and must be provided
    /// as a cascading parameter from a parent component.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown if this parameter is not available when the component initializes.
    /// </exception>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the action to authorize for this button.
    /// This parameter is required and defines what operation the user is attempting to perform.
    /// </summary>
    /// <value>
    /// The action name to check permissions for (e.g., "read", "write", "delete", "create").
    /// This parameter is required and must be specified.
    /// </value>
    /// <remarks>
    /// <para>
    /// Actions typically represent verbs describing what operation the user wants to perform.
    /// Common actions include "read", "create", "update", "delete", or custom business-specific
    /// actions like "publish", "approve", or "archive". The action is evaluated against the
    /// current user's privileges to determine if the button should be enabled.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Action { get; set; }

    /// <summary>
    /// Gets or sets the subject to authorize for this button.
    /// The subject typically represents a resource, entity type, or domain object.
    /// When null, empty, or whitespace, all privileges are assumed to be granted.
    /// </summary>
    /// <value>
    /// The subject name representing the resource or entity to check permissions for (e.g., "Post", "User", "Order").
    /// Can be <c>null</c> for context-free actions.
    /// </value>
    /// <remarks>
    /// <para>
    /// The subject represents the target of the action - what the user is trying to perform the action on.
    /// For instance, in a content management system, subjects might include "Post", "Page", "User", or "Comment".
    /// </para>
    /// <para>
    /// <strong>Special behavior:</strong> When the Subject parameter is <c>null</c>, empty, or contains only whitespace,
    /// the component assumes all privileges are granted and will enable the button regardless
    /// of the privilege context rules. This allows for fallback behavior when subject information is not available.
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets an optional qualifier that provides additional scoping for the privilege evaluation.
    /// This allows for fine-grained permission control at the field, property, or operation level.
    /// </summary>
    /// <value>
    /// An optional qualifier string that further scopes the privilege check
    /// (e.g., a field name, category, or specific identifier). Can be <c>null</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// The qualifier parameter allows for granular permission control beyond the action-subject level.
    /// For instance, a user might have permission to "update" a "Post" but only specific fields like
    /// "title" and "summary", not sensitive fields like "metadata" or "internalNotes".
    /// </para>
    /// </remarks>
    [Parameter]
    public string? Qualifier { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the button is in a busy state.
    /// When <c>true</c>, the busy indicator is shown and the button is disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the button should display busy state and be disabled; otherwise, <c>false</c>.
    /// Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter allows external control of the button's busy state, independent of the
    /// internal <see cref="Executing"/> state that is managed automatically during <see cref="Trigger"/> execution.
    /// </para>
    /// </remarks>
    [Parameter]
    public bool Busy { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the button is disabled.
    /// When <c>true</c>, the button cannot be clicked and appears visually disabled.
    /// </summary>
    /// <value>
    /// <c>true</c> if the button should be disabled; otherwise, <c>false</c>.
    /// Defaults to <c>false</c>.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter provides explicit control over the button's disabled state, independent of
    /// privilege checks, busy states, or other automatic disabling conditions.
    /// </para>
    /// <para>
    /// The final disabled state of the button is determined by combining this parameter with
    /// other conditions: the button is disabled if any of the following are true:
    /// <list type="bullet">
    /// <item><description>This <see cref="Disabled"/> parameter is <c>true</c></description></item>
    /// <item><description>The button is busy (see <see cref="IsBusy"/>)</description></item>
    /// <item><description>The user lacks the required permission (see <see cref="HasPermission"/>)</description></item>
    /// </list>
    /// </para>
    /// </remarks>
    [Parameter]
    public bool Disabled { get; set; }

    /// <summary>
    /// Gets or sets the text to display when the button is busy.
    /// This text is shown in the default busy template if no custom <see cref="BusyTemplate"/> is provided.
    /// </summary>
    /// <value>
    /// The text to display during busy state. Defaults to "Processing...".
    /// </value>
    [Parameter]
    public string BusyText { get; set; } = "Processing...";

    /// <summary>
    /// Gets or sets a custom template to display when the button is busy.
    /// When provided, this template overrides the default busy display that uses <see cref="BusyText"/>.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to render when the button is busy.
    /// Can be <c>null</c> to use the default busy template.
    /// </value>
    [Parameter]
    public RenderFragment? BusyTemplate { get; set; }

    /// <summary>
    /// Gets or sets the content to display inside the button when it is not busy.
    /// This represents the normal, interactive state of the button.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to render when the button is in its normal state.
    /// Can be <c>null</c> if the button should be empty when not busy.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets additional attributes to be applied to the button element.
    /// This allows for full customization of the HTML button's appearance and behavior.
    /// </summary>
    /// <value>
    /// A dictionary containing HTML attribute names and their corresponding values.
    /// Defaults to an empty dictionary.
    /// </value>
    [Parameter(CaptureUnmatchedValues = true)]
    public Dictionary<string, object> AdditionalAttributes { get; set; } = [];

    /// <summary>
    /// Gets or sets the event callback to trigger when the button is clicked.
    /// The button automatically manages busy state during callback execution.
    /// </summary>
    /// <value>
    /// An <see cref="EventCallback"/> that is invoked when the button is clicked.
    /// Can be empty if the button should not perform any action.
    /// </value>
    /// <remarks>
    /// <para>
    /// This parameter defines the action to be performed when the user clicks the button.
    /// The callback can be synchronous or asynchronous, and the component automatically
    /// manages the busy state during execution.
    /// </para>
    /// </remarks>
    [Parameter]
    public EventCallback Trigger { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether buttons should be hidden when the user
    /// lacks the required permissions instead of being displayed in a disabled state.
    /// </summary>
    /// <value>
    /// <c>true</c> to hide the button when permission is denied; <c>false</c> to show
    /// the button in a disabled state. Defaults to <c>false</c>.
    /// </value>
    [Parameter]
    public bool HideForbidden { get; set; }

    /// <summary>
    /// Gets a value indicating whether the button is currently executing the <see cref="Trigger"/> callback.
    /// This is an internal state managed automatically by the component.
    /// </summary>
    /// <value>
    /// <c>true</c> if the trigger callback is currently executing; otherwise, <c>false</c>.
    /// </value>
    private bool Executing { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has permission to access this button
    /// based on the specified <see cref="Action"/>, <see cref="Subject"/>, and optional <see cref="Qualifier"/>.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has the required permission for the specified parameters;
    /// otherwise, <c>false</c>.
    /// </value>
    protected bool HasPermission { get; set; }

    /// <summary>
    /// Gets a value indicating whether the button is currently in a busy state.
    /// This combines both external busy state and internal execution state.
    /// </summary>
    /// <value>
    /// <c>true</c> if the button is busy due to the <see cref="Busy"/> parameter being <c>true</c>
    /// or the <see cref="Trigger"/> callback is currently executing; otherwise, <c>false</c>.
    /// </value>
    protected bool IsBusy => Busy || Executing;

    /// <summary>
    /// Builds the render tree for the PrivilegeButton component, creating the button element
    /// with appropriate attributes, event handlers, and content based on the current state.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the component's render tree.</param>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        // Do not render if the user does not have permission and HideForbidden is true
        if (HideForbidden && !HasPermission)
            return;

        builder.OpenElement(0, "button");
        builder.AddMultipleAttributes(1, AdditionalAttributes);
        builder.AddAttribute(2, "disabled", Disabled || IsBusy || !HasPermission);

        if (Trigger.HasDelegate)
        {
            builder.AddAttribute(3, "type", "button");
            builder.AddAttribute(4, "onclick", EventCallback.Factory.Create<MouseEventArgs>(this, ExecuteTrigger));
        }

        if (IsBusy && BusyTemplate != null)
            builder.AddContent(5, BusyTemplate);
        else
            builder.AddContent(6, ChildContent);

        builder.CloseElement(); // button
    }

    /// <summary>
    /// Called when the component's parameters are set or updated. This method validates the component
    /// configuration, evaluates user privileges, and initializes default templates.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the required <see cref="PrivilegeContext"/> cascading parameter is not provided or is <c>null</c>.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PrivilegeContext == null)
            throw new InvalidOperationException("Component requires a cascading parameter of type PrivilegeContext.");

        BusyTemplate ??= builder => builder.AddContent(0, BusyText);

        HasPermission = string.IsNullOrWhiteSpace(Action)
            || string.IsNullOrWhiteSpace(Subject)
            || PrivilegeContext.Allowed(Action, Subject, Qualifier);
    }

    /// <summary>
    /// Executes the <see cref="Trigger"/> callback and manages the busy state during execution.
    /// This method provides automatic busy state management and exception handling.
    /// </summary>
    /// <returns>
    /// A <see cref="Task"/> representing the asynchronous operation.
    /// </returns>
    private async Task ExecuteTrigger()
    {
        if (!Trigger.HasDelegate)
            return;

        try
        {
            Executing = true;
            await Trigger.InvokeAsync();
        }
        finally
        {
            Executing = false;
        }
    }
}
