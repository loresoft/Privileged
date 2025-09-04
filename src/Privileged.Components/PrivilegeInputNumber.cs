using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components;

/// <summary>
/// A custom input number component that integrates privilege-based access control with forms.
/// Automatically evaluates read and update permissions based on the current <see cref="PrivilegeContext"/>
/// and renders appropriate UI elements based on the user's access level.
/// </summary>
/// <typeparam name="TValue">The type of the numeric value bound to the input number. This must be a numeric type such as <see cref="int"/>, <see cref="decimal"/>, <see cref="double"/>, etc.</typeparam>
/// <remarks>
/// <para>
/// This component extends the standard <see cref="InputNumber{TValue}"/> component to provide privilege-aware
/// functionality. It automatically:
/// </para>
/// <list type="bullet">
/// <item><description>Evaluates read and update permissions based on cascading privilege context</description></item>
/// <item><description>Renders a password input field when read permission is denied to hide numeric values</description></item>
/// <item><description>Sets readonly attribute when update permission is denied but read permission is granted</description></item>
/// <item><description>Inherits all standard InputNumber functionality when permissions are granted</description></item>
/// </list>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly.
/// It can optionally use a cascading <see cref="PrivilegeFormState"/> for default privilege settings.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PrivilegeInputNumber @bind-Value="model.Price"
///                       Subject="Product"
///                       Field="Price"
///                       step="0.01" /&gt;
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeFormState"/>
/// <seealso cref="InputNumber{TValue}"/>
public class PrivilegeInputNumber<TValue> : InputNumber<TValue>
{
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
    /// The subject typically represents the entity or resource type being accessed (e.g., "Product", "Invoice", "Order").
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
    /// representing the specific numeric field or property being accessed (e.g., "Price", "Quantity", "Amount", "Score").
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
    /// This action is used to determine if the user can view the numeric input's value.
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
    /// This action is used to determine if the user can modify the numeric input's value.
    /// Common update actions include "update", "edit", "modify", or "write".
    /// </remarks>
    [Parameter]
    public string? UpdateAction { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has read permission for the numeric input.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has read permission; otherwise, <c>false</c>.
    /// This value is calculated during <see cref="OnParametersSet"/> based on the current privilege context.
    /// </value>
    /// <remarks>
    /// When this property is <c>false</c>, the component renders a disabled password input
    /// instead of showing the actual numeric value to maintain data confidentiality.
    /// </remarks>
    protected bool HasReadPermission { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has update permission for the numeric input.
    /// </summary>
    /// <value>
    /// <c>true</c> if the user has update permission; otherwise, <c>false</c>.
    /// This value is calculated during <see cref="OnParametersSet"/> based on the current privilege context.
    /// </value>
    /// <remarks>
    /// When this property is <c>false</c>, the component adds a readonly attribute to prevent editing,
    /// assuming the user has read permission. If the user lacks both read and update permissions,
    /// a password input is rendered instead.
    /// </remarks>
    protected bool HasUpdatePermission { get; set; }

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
    /// If the user lacks update permission, a readonly attribute is automatically added to the numeric input.
    /// If the subject is null, empty, or whitespace, all privileges are assumed to be granted.
    /// </para>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> cascading parameter is not provided.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PrivilegeContext == null)
            throw new InvalidOperationException($"Component requires a cascading parameter of type {nameof(PrivilegeContext)}.");

        var subject = Subject ?? PrivilegeFormState?.Subject ?? EditContext?.Model.GetType().Name;
        var qualifier = Field ?? NameAttributeValue;
        var readAction = ReadAction ?? PrivilegeFormState?.ReadAction ?? "read";
        var updateAction = UpdateAction ?? PrivilegeFormState?.UpdateAction ?? "update";

        HasReadPermission = string.IsNullOrWhiteSpace(subject)
            || PrivilegeContext.Allowed(readAction, subject, qualifier);

        HasUpdatePermission = string.IsNullOrWhiteSpace(subject)
            || PrivilegeContext.Allowed(updateAction, subject, qualifier);

        if (HasUpdatePermission)
            return;

        // If no update permission, set readonly or disabled attributes
        var attributes = AdditionalAttributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
        if (!HasUpdatePermission)
            attributes["readonly"] = true;

        AdditionalAttributes = attributes;
    }

    /// <summary>
    /// Builds the render tree for the component, rendering different input types based on user permissions.
    /// </summary>
    /// <param name="builder">The <see cref="Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder"/> used to build the render tree.</param>
    /// <remarks>
    /// <para>
    /// The rendering behavior depends on the user's permissions:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Has read permission:</strong> Renders the standard <see cref="InputNumber{TValue}"/> component with all inherited functionality</description></item>
    /// <item><description><strong>No read permission:</strong> Renders a disabled password input field that hides the actual numeric value</description></item>
    /// </list>
    /// <para>
    /// When rendering a password input (no read permission), the component:
    /// </para>
    /// <list type="bullet">
    /// <item><description>Sets the input type to "password" to mask the numeric content</description></item>
    /// <item><description>Disables the input to prevent any interaction</description></item>
    /// <item><description>Preserves the current value, name, CSS class, and additional attributes</description></item>
    /// <item><description>Maintains the element reference for form integration</description></item>
    /// </list>
    /// <para>
    /// Note that when no read permission is granted, a single-line password input is rendered instead of
    /// a numeric input to maintain security and hide sensitive numeric data such as prices, salaries, or financial amounts.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        // If the user has read permission, render the base InputNumber component
        if (HasReadPermission)
        {
            base.BuildRenderTree(builder);
            return;
        }

        // If the user does not have read permission, render a password input
        builder.OpenElement(0, "input");
        builder.AddAttribute(1, "type", "password");
        builder.AddAttribute(2, "disabled", true);
        builder.AddMultipleAttributes(3, AdditionalAttributes);

        if (!string.IsNullOrEmpty(NameAttributeValue))
            builder.AddAttribute(4, "name", NameAttributeValue);

        if (!string.IsNullOrEmpty(CssClass))
            builder.AddAttribute(5, "class", CssClass);

        builder.AddAttribute(6, "value", CurrentValueAsString);
        builder.AddElementReferenceCapture(7, __inputReference => Element = __inputReference);
        builder.CloseElement();
    }
}
