using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A custom form component that extends <see cref="EditForm"/> to provide privilege-aware form functionality.
/// Automatically cascades privilege form state to child components for consistent privilege evaluation.
/// </summary>
/// <remarks>
/// <para>
/// This component serves as a wrapper around the standard <see cref="EditForm"/> component, adding
/// privilege-aware capabilities by cascading a <see cref="Components.PrivilegeFormState"/> to all child components.
/// This allows child privilege components (such as <see cref="PrivilegeInputText"/>) to inherit
/// default subject, read action, and update action values.
/// </para>
/// <para>
/// The component maintains all the functionality of the base <see cref="EditForm"/> while adding
/// privilege context management. Child components can override the cascaded values by specifying
/// their own parameter values.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PrivilegeForm Model="userModel"
///               Subject="User"
///               ReadAction="view"
///               UpdateAction="edit"&gt;
///     &lt;PrivilegeInputText @bind-Value="userModel.FirstName" /&gt;
///     &lt;PrivilegeInputText @bind-Value="userModel.Email" Field="ContactEmail" /&gt;
/// &lt;/PrivilegeForm&gt;
/// </code>
/// </example>
/// <seealso cref="EditForm"/>
/// <seealso cref="Components.PrivilegeFormState"/>
/// <seealso cref="PrivilegeInputText"/>
public class PrivilegeForm : EditForm
{
    /// <summary>
    /// Gets or sets the default subject for privilege evaluation that will be cascaded to child components.
    /// </summary>
    /// <value>
    /// The subject string used as the default for privilege evaluation in child components.
    /// When <c>null</c>, child components will attempt to determine the subject from other sources
    /// such as the edit context model's type name.
    /// </value>
    /// <remarks>
    /// This value is cascaded to all child privilege components as part of the <see cref="Components.PrivilegeFormState"/>.
    /// Individual child components can override this value by specifying their own Subject parameter.
    /// The subject typically represents the entity or resource type being accessed (e.g., "User", "Document", "Order").
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the default read action for privilege evaluation that will be cascaded to child components.
    /// </summary>
    /// <value>
    /// The action string used as the default for read privilege evaluation in child components.
    /// When <c>null</c>, child components will use their own default read action (typically "read").
    /// </value>
    /// <remarks>
    /// This value is cascaded to all child privilege components as part of the <see cref="Components.PrivilegeFormState"/>.
    /// Individual child components can override this value by specifying their own ReadAction parameter.
    /// Common read actions include "read", "view", or "get".
    /// </remarks>
    [Parameter]
    public string? ReadAction { get; set; }

    /// <summary>
    /// Gets or sets the default update action for privilege evaluation that will be cascaded to child components.
    /// </summary>
    /// <value>
    /// The action string used as the default for update privilege evaluation in child components.
    /// When <c>null</c>, child components will use their own default update action (typically "update").
    /// </value>
    /// <remarks>
    /// This value is cascaded to all child privilege components as part of the <see cref="Components.PrivilegeFormState"/>.
    /// Individual child components can override this value by specifying their own UpdateAction parameter.
    /// Common update actions include "update", "edit", "modify", or "write".
    /// </remarks>
    [Parameter]
    public string? UpdateAction { get; set; }

    /// <summary>
    /// Gets the privilege form state that encapsulates the default privilege settings for child components.
    /// </summary>
    /// <value>
    /// The <see cref="Components.PrivilegeFormState"/> instance that contains the current subject, read action,
    /// and update action values. This state is automatically created during parameter initialization
    /// and cascaded to child components.
    /// </value>
    /// <remarks>
    /// This property is initialized during <see cref="OnParametersSet"/> and is used internally
    /// to cascade privilege settings to child components via a <see cref="CascadingValue{TValue}"/>.
    /// </remarks>
    protected PrivilegeFormState PrivilegeFormState { get; set; } = null!;

    /// <summary>
    /// Called when the component's parameters are set. Creates and initializes the privilege form state.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Calls the base implementation to ensure proper <see cref="EditForm"/> initialization</description></item>
    /// <item><description>Creates a new <see cref="Components.PrivilegeFormState"/> instance with current parameter values</description></item>
    /// <item><description>Assigns the form state for cascading to child components</description></item>
    /// </list>
    /// <para>
    /// The created form state will be cascaded to all child components, allowing them to inherit
    /// the default privilege settings while maintaining the ability to override individual values.
    /// </para>
    /// </remarks>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        PrivilegeFormState = new PrivilegeFormState(
            Subject,
            ReadAction,
            UpdateAction
        );

    }

    /// <summary>
    /// Builds the render tree for the component, wrapping the base <see cref="EditForm"/> functionality
    /// with a cascading value provider for privilege form state.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the render tree.</param>
    /// <remarks>
    /// <para>
    /// This method creates a render tree structure that:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Wraps the base <see cref="EditForm"/> content with a <see cref="CascadingValue{TValue}"/> component</description></item>
    /// <item><description>Cascades the <see cref="PrivilegeFormState"/> to all child components</description></item>
    /// <item><description>Preserves all base <see cref="EditForm"/> functionality and rendering behavior</description></item>
    /// </list>
    /// <para>
    /// Child privilege components can access the cascaded <see cref="Components.PrivilegeFormState"/> through
    /// a <see cref="CascadingParameterAttribute"/> to inherit default privilege settings.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<PrivilegeFormState>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<>.Value), PrivilegeFormState);
        builder.AddAttribute(2, nameof(CascadingValue<>.IsFixed), true);
        builder.AddAttribute(3, nameof(CascadingValue<>.ChildContent), (RenderFragment)(childBuilder => base.BuildRenderTree(childBuilder)));
        builder.CloseComponent();
    }
}
