using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components;

/// <summary>
/// A custom password input component that integrates privilege-based access control with Blazor forms.
/// Automatically evaluates read and update permissions based on the current <see cref="PrivilegeContext"/>
/// and renders appropriate UI elements based on the user's access level.
/// </summary>
/// <remarks>
/// <para>
/// This component extends the standard <see cref="InputText"/> component to provide privilege-aware
/// password input functionality. It automatically:
/// </para>
/// <list type="bullet">
/// <item><description>Evaluates read and update permissions based on cascading privilege context</description></item>
/// <item><description>Renders a disabled password input field when read permission is denied</description></item>
/// <item><description>Sets readonly attribute when update permission is denied but read permission is granted</description></item>
/// <item><description>Optionally provides a toggle button to show/hide password text when enabled</description></item>
/// <item><description>Inherits all standard InputText functionality when permissions are granted</description></item>
/// </list>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly.
/// It can optionally use a cascading <see cref="PrivilegeFormState"/> for default privilege settings.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PrivilegeInputPassword @bind-Value="model.Password"
///                          Subject="User"
///                          Field="Password"
///                          Togglable="true" /&gt;
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeFormState"/>
/// <seealso cref="InputText"/>
public class PrivilegeInputPassword : InputText
{
    private const string ShowIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M2.036 12.322a1.012 1.012 0 010-.639C3.423 7.51 7.36 4.5 12 4.5c4.638 0 8.573 3.007 9.963 7.178.07.207.07.431 0 .639C20.577 16.49 16.64 19.5 12 19.5c-4.638 0-8.573-3.007-9.963-7.178z'/><path stroke-linecap='round' stroke-linejoin='round' d='M15 12a3 3 0 11-6 0 3 3 0 016 0z'/></svg>";
    private const string HideIcon = "<svg xmlns='http://www.w3.org/2000/svg' fill='none' viewBox='0 0 24 24' stroke-width='2' stroke='currentColor' width='20' height='20'><path stroke-linecap='round' stroke-linejoin='round' d='M3.98 8.223A10.477 10.477 0 001.934 12C3.226 16.338 7.244 19.5 12 19.5c.993 0 1.953-.138 2.863-.395M6.228 6.228A10.45 10.45 0 0112 4.5c4.756 0 8.773 3.162 10.065 7.498a10.523 10.523 0 01-4.293 5.774M6.228 6.228L3 3m3.228 3.228l3.65 3.65m7.894 7.894L21 21m-3.228-3.228l-3.65-3.65m0 0a3 3 0 10-4.243-4.243m4.242 4.242L9.88 9.88'/></svg>";
    private const string ToggleButtonStyle = "position: absolute; right: 0; top: 0; bottom: 0; background: none; border: none; cursor: pointer; display: flex; align-items: center; justify-content: center;";

    private bool _passwordVisible = false;

    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// This parameter is required for the component to function properly.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeContext"/> instance that contains the privilege rules and aliases
    /// used for authorization checks. Cannot be <c>null</c>.
    /// </value>
    /// <exception cref="InvalidOperationException">
    /// Thrown during <see cref="OnParametersSet"/> if this parameter is <c>null</c>.
    /// </exception>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the cascading privilege form state that provides default privilege settings.
    /// This parameter is optional and provides default values for subject, read action, and update action.
    /// </summary>
    /// <value>
    /// The <see cref="PrivilegeFormState"/> instance that contains default privilege settings,
    /// or <c>null</c> if no form state is available.
    /// </value>
    [CascadingParameter]
    protected PrivilegeFormState? PrivilegeFormState { get; set; }

    /// <summary>
    /// Gets or sets the subject for privilege evaluation.
    /// </summary>
    /// <value>
    /// The subject string used for privilege evaluation. If not specified, defaults to:
    /// <list type="number">
    /// <item><description>The <see cref="PrivilegeFormState.Subject"/> if available</description></item>
    /// <item><description>The edit context model's type name if available</description></item>
    /// <item><description><c>null</c> if none of the above are available</description></item>
    /// </list>
    /// When <c>null</c>, empty, or whitespace, all privileges are assumed to be granted.
    /// </value>
    /// <remarks>
    /// The subject typically represents the entity or resource type being accessed (e.g., "User", "Document", "Order").
    /// </remarks>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the field qualifier for privilege evaluation.
    /// </summary>
    /// <value>
    /// The field string used as a qualifier for privilege evaluation. If not specified,
    /// defaults to the input's name attribute value (<see cref="InputBase{TValue}.NameAttributeValue"/>).
    /// </value>
    /// <remarks>
    /// The field qualifier provides additional context for privilege evaluation, typically
    /// representing the specific field or property being accessed (e.g., "FirstName", "Email", "Salary").
    /// </remarks>
    [Parameter]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the action name for read permissions.
    /// </summary>
    /// <value>
    /// The action string used for read privilege evaluation. If not specified, defaults to:
    /// <list type="number">
    /// <item><description>The <see cref="PrivilegeFormState.ReadAction"/> if available</description></item>
    /// <item><description>"read" as the default value</description></item>
    /// </list>
    /// </value>
    /// <remarks>
    /// This action is used to determine if the user can view the input field's value.
    /// Common read actions include "read", "view", or "get".
    /// </remarks>
    [Parameter]
    public string? ReadAction { get; set; }

    /// <summary>
    /// Gets or sets the action name for update permissions.
    /// </summary>
    /// <value>
    /// The action string used for update privilege evaluation. If not specified, defaults to:
    /// <list type="number">
    /// <item><description>The <see cref="PrivilegeFormState.UpdateAction"/> if available</description></item>
    /// <item><description>"update" as the default value</description></item>
    /// </list>
    /// </value>
    /// <remarks>
    /// This action is used to determine if the user can modify the input field's value.
    /// Common update actions include "update", "edit", "modify", or "write".
    /// </remarks>
    [Parameter]
    public string? UpdateAction { get; set; }

    /// <summary>
    /// Gets or sets the CSS class to apply to the container div that wraps the input and toggle button.
    /// </summary>
    /// <value>
    /// A CSS class string to apply to the container element, or <c>null</c> if no custom class is specified.
    /// </value>
    [Parameter]
    public string? ContainerClass { get; set; }

    /// <summary>
    /// Gets or sets the CSS class to apply to the password visibility toggle button.
    /// </summary>
    /// <value>
    /// A CSS class string to apply to the toggle button element, or <c>null</c> if no custom class is specified.
    /// </value>
    [Parameter]
    public string? ToggleClass { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the password visibility toggle button should be displayed.
    /// </summary>
    /// <value>
    /// <c>true</c> to display a toggle button that allows showing/hiding the password text; otherwise, <c>false</c>.
    /// The default is <c>false</c>.
    /// </value>
    /// <remarks>
    /// The toggle button is only displayed when this property is <c>true</c> and the user has read permission.
    /// If the user lacks read permission, the toggle button is not rendered regardless of this setting.
    /// </remarks>
    [Parameter]
    public bool Togglable { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has read permission for the password field.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has read permission; otherwise, <c>false</c>.
    /// This value is calculated during <see cref="OnParametersSet"/> based on the current privilege context.
    /// </value>
    /// <remarks>
    /// When this property is <c>false</c>, the component renders a disabled password input
    /// to prevent viewing the actual field value.
    /// </remarks>
    protected bool HasReadPermission { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has update permission for the password field.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has update permission; otherwise, <c>false</c>.
    /// This value is calculated during <see cref="OnParametersSet"/> based on the current privilege context.
    /// </value>
    /// <remarks>
    /// When this property is <c>false</c>, the component adds a readonly attribute to prevent editing,
    /// assuming the user has read permission. If the user lacks both read and update permissions,
    /// a disabled password input is rendered instead.
    /// </remarks>
    protected bool HasUpdatePermission { get; set; }

    /// <summary>
    /// Gets the HTML input type based on the current password visibility state.
    /// </summary>
    /// <value>
    /// Returns "text" when the password is visible; otherwise returns "password".
    /// </value>
    /// <remarks>
    /// This property is used to dynamically switch between showing password text as plain text
    /// or masking it with password dots when the user toggles visibility.
    /// </remarks>
    protected string InputType => _passwordVisible ? "text" : "password";

    /// <summary>
    /// Called when the component's parameters are set. Evaluates permissions and adjusts attributes accordingly.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Calls the base implementation to ensure proper initialization</description></item>
    /// <item><description>Validates that a <see cref="PrivilegeContext"/> is available</description></item>
    /// <item><description>Resolves default values for subject, field, read action, and update action</description></item>
    /// <item><description>Evaluates read and update permissions using the privilege context</description></item>
    /// <item><description>Modifies component attributes based on permission results</description></item>
    /// </list>
    /// <para>
    /// If the user lacks update permission, a readonly attribute is automatically added to the input.
    /// If the subject is null, empty, or whitespace, all privileges are assumed to be granted.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> cascading parameter is not provided.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        var subject = Subject ?? PrivilegeFormState?.Subject ?? EditContext?.Model.GetType().Name;
        var qualifier = Field ?? NameAttributeValue;
        var readAction = ReadAction ?? PrivilegeFormState?.ReadAction ?? "read";
        var updateAction = UpdateAction ?? PrivilegeFormState?.UpdateAction ?? "update";

        HasReadPermission = PrivilegeContext == null
            || string.IsNullOrWhiteSpace(subject)
            || PrivilegeContext.Allowed(readAction, subject, qualifier);

        HasUpdatePermission = PrivilegeContext == null
            || string.IsNullOrWhiteSpace(subject)
            || PrivilegeContext.Allowed(updateAction, subject, qualifier);
    }

    /// <summary>
    /// Builds the render tree for the component, rendering a password input with optional visibility toggle based on user permissions.
    /// </summary>
    /// <param name="builder">The <see cref="Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder"/> used to build the render tree.</param>
    /// <remarks>
    /// <para>
    /// The rendering behavior depends on the user's permissions:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Has read permission:</strong> Renders a password input that can optionally be toggled to show/hide text</description></item>
    /// <item><description><strong>Has update permission:</strong> Renders an editable password input with two-way data binding</description></item>
    /// <item><description><strong>No read permission:</strong> Renders a disabled password input field that prevents viewing the value</description></item>
    /// <item><description><strong>No update permission:</strong> Adds a readonly attribute to prevent editing while still allowing visibility (if read permission exists)</description></item>
    /// </list>
    /// <para>
    /// The component renders a container div with the following structure:
    /// </para>
    /// <list type="bullet">
    /// <item><description>A password input element with appropriate type, disabled, and readonly attributes</description></item>
    /// <item><description>An optional toggle button (when <see cref="Togglable"/> is true and user has read permission) that switches between showing and hiding password text</description></item>
    /// </list>
    /// <para>
    /// The toggle button displays eye icons (show/hide) using inline SVG and is positioned absolutely within the container.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        var showToggle = HasReadPermission && Togglable;

        // only wrap in div if needed for toggle button
        if (showToggle)
        {
            // <div>
            builder.OpenElement(0, "div");
            builder.AddAttribute(1, "style", "position: relative;");
            if (!string.IsNullOrEmpty(ContainerClass))
                builder.AddAttribute(2, "class", ContainerClass);

        }

        // <input>
        builder.OpenElement(3, "input");
        builder.AddAttribute(4, "type", InputType);

        if (!HasReadPermission)
            builder.AddAttribute(5, "disabled", true);

        if (!HasUpdatePermission)
            builder.AddAttribute(6, "readonly", true);

        builder.AddMultipleAttributes(7, AdditionalAttributes);

        if (!string.IsNullOrEmpty(NameAttributeValue))
            builder.AddAttribute(8, "name", NameAttributeValue);

        if (!string.IsNullOrEmpty(CssClass))
            builder.AddAttribute(9, "class", CssClass);

        builder.AddAttribute(10, "value", CurrentValueAsString);

        if (HasUpdatePermission)
        {
            builder.AddAttribute(11, "onchange", EventCallback.Factory.CreateBinder<string?>(this, __value => CurrentValueAsString = __value, CurrentValueAsString));
            builder.SetUpdatesAttributeName("value");
        }

        // Add padding to the right if toggle button is shown
        if (showToggle)
            builder.AddAttribute(12, "style", "padding-right: 2rem;");

        builder.AddElementReferenceCapture(13, __inputReference => Element = __inputReference);
        builder.CloseElement(); // </input>

        // show toggle button
        if (showToggle)
        {
            // <button>
            builder.OpenElement(14, "button");
            builder.AddAttribute(15, "type", "button");
            builder.AddAttribute(16, "style", ToggleButtonStyle);

            if (!string.IsNullOrEmpty(ToggleClass))
                builder.AddAttribute(17, "class", ToggleClass);

            builder.AddAttribute(18, "onclick", EventCallback.Factory.Create(this, ToggleVisibility));
            builder.AddAttribute(19, "title", "Toggle Visibility");

            // Add the appropriate icon based on visibility state
            builder.AddMarkupContent(20, _passwordVisible ? HideIcon : ShowIcon);

            builder.CloseElement(); // </button>

            builder.CloseElement(); // </div>
        }

    }

    private void ToggleVisibility()
    {
        _passwordVisible = !_passwordVisible;
        StateHasChanged();
    }
}
