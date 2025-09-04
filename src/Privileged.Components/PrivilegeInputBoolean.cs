using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A custom input component for selecting boolean values (<c>bool</c> or <c>bool?</c>) that integrates privilege-based access control with Blazor forms.
/// Automatically evaluates read and update permissions based on the current <see cref="PrivilegeContext"/>
/// and renders appropriate UI elements based on the user's access level.
/// </summary>
/// <typeparam name="TValue">The type of the value bound to the input. Must be <c>bool</c> or <c>bool?</c>.</typeparam>
/// <remarks>
/// <para>
/// This component extends the <see cref="PrivilegeInputSelect{TValue}"/> component to provide a specialized
/// boolean selection interface with privilege-aware functionality. It automatically:
/// </para>
/// <list type="bullet">
/// <item><description>Generates appropriate select options for boolean values (True/False, with optional null option for nullable booleans)</description></item>
/// <item><description>Evaluates read and update permissions based on cascading privilege context (inherited from base class)</description></item>
/// <item><description>Renders a password input field when read permission is denied to hide content</description></item>
/// <item><description>Disables the select element when update permission is denied but read permission is granted</description></item>
/// <item><description>Provides customizable labels for True, False, and null options</description></item>
/// </list>
/// <para>
/// The component requires a cascading <see cref="PrivilegeContext"/> parameter to function properly (inherited requirement).
/// It can optionally use a cascading <see cref="PrivilegeFormState"/> for default privilege settings.
/// </para>
/// <para>
/// Type safety is enforced at runtime to ensure only <c>bool</c> or <c>bool?</c> types are used with this component.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PrivilegeInputBoolean @bind-Value="model.IsActive" 
///                        Subject="User" 
///                        Field="IsActive"
///                        TrueLabel="Enabled"
///                        FalseLabel="Disabled" /&gt;
/// 
/// &lt;!-- For nullable boolean with custom null label --&gt;
/// &lt;PrivilegeInputBoolean @bind-Value="model.IsApproved" 
///                        Subject="Document" 
///                        Field="IsApproved"
///                        NullLabel="Pending Review"
///                        TrueLabel="Approved"
///                        FalseLabel="Rejected" /&gt;
/// </code>
/// </example>
/// <seealso cref="PrivilegeInputSelect{TValue}"/>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeFormState"/>
public class PrivilegeInputBoolean<TValue> : PrivilegeInputSelect<TValue>
{
    /// <summary>
    /// Gets or sets the label to display for the null option (only shown for <c>bool?</c>).
    /// </summary>
    /// <value>
    /// The text to display for the null/unselected option when <typeparamref name="TValue"/> is <c>bool?</c>.
    /// Defaults to "- select -".
    /// </value>
    /// <remarks>
    /// This option is only rendered when the bound type is <c>bool?</c> (nullable boolean).
    /// For non-nullable <c>bool</c> types, this parameter is ignored since a null option is not applicable.
    /// </remarks>
    [Parameter]
    public string NullLabel { get; set; } = "- select -";

    /// <summary>
    /// Gets or sets the label to display for the true option.
    /// </summary>
    /// <value>
    /// The text to display for the <c>true</c> value option. Defaults to "True".
    /// </value>
    /// <remarks>
    /// This label can be customized to provide more meaningful text for the specific use case,
    /// such as "Enabled", "Active", "Yes", "Approved", etc.
    /// </remarks>
    [Parameter]
    public string TrueLabel { get; set; } = "True";

    /// <summary>
    /// Gets or sets the label to display for the false option.
    /// </summary>
    /// <value>
    /// The text to display for the <c>false</c> value option. Defaults to "False".
    /// </value>
    /// <remarks>
    /// This label can be customized to provide more meaningful text for the specific use case,
    /// such as "Disabled", "Inactive", "No", "Rejected", etc.
    /// </remarks>
    [Parameter]
    public string FalseLabel { get; set; } = "False";

    /// <summary>
    /// Called when component parameters are set. Ensures only <c>bool</c> or <c>bool?</c> are allowed and sets the default child content.
    /// </summary>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Calls the base implementation to handle privilege evaluation and attribute setup</description></item>
    /// <item><description>Validates that <typeparamref name="TValue"/> is either <c>bool</c> or <c>bool?</c></description></item>
    /// <item><description>Sets the default <see cref="Microsoft.AspNetCore.Components.Forms.InputSelect{TValue}.ChildContent"/> to render boolean options if not already specified</description></item>
    /// </list>
    /// <para>
    /// The child content is automatically generated to include:
    /// </para>
    /// <list type="bullet">
    /// <item><description>A null option (for <c>bool?</c> types only) with the <see cref="NullLabel"/> text</description></item>
    /// <item><description>A true option with the <see cref="TrueLabel"/> text</description></item>
    /// <item><description>A false option with the <see cref="FalseLabel"/> text</description></item>
    /// </list>
    /// </remarks>
    /// <exception cref="InvalidOperationException">
    /// Thrown if <typeparamref name="TValue"/> is not <c>bool</c> or <c>bool?</c>.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (typeof(TValue) != typeof(bool) && typeof(TValue) != typeof(bool?))
            throw new InvalidOperationException("Component only supports bool or bool? types.");

        ChildContent ??= RenderBooleanOptions;
    }

    /// <summary>
    /// Converts the value to a string for rendering in the select element.
    /// </summary>
    /// <param name="value">The value to format for HTML rendering.</param>
    /// <returns>
    /// <list type="bullet">
    /// <item><description>"true" for <c>true</c> boolean values</description></item>
    /// <item><description>"false" for <c>false</c> boolean values or when <paramref name="value"/> is <c>null</c> for non-nullable types</description></item>
    /// <item><description><c>null</c> for <c>null</c> values when <typeparamref name="TValue"/> is <c>bool?</c></description></item>
    /// <item><description>Result of base implementation for other types (fallback)</description></item>
    /// </list>
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method handles the conversion of boolean values to their string representations for HTML select elements:
    /// </para>
    /// <list type="bullet">
    /// <item><description>For <c>bool</c> types: Always returns "true" or "false" (defaults to "false" for unexpected null values)</description></item>
    /// <item><description>For <c>bool?</c> types: Returns "true", "false", or <c>null</c> based on the actual value</description></item>
    /// </list>
    /// <para>
    /// The returned string values correspond to the "value" attributes of the generated option elements.
    /// </para>
    /// </remarks>
    protected override string? FormatValueAsString(TValue? value)
    {
        if (typeof(TValue) == typeof(bool))
        {
            if (value is bool b)
                return b ? "true" : "false";

            return "false";
        }
        else if (typeof(TValue) == typeof(bool?))
        {
            if (value is null)
                return null;

            if (value is bool b)
                return b ? "true" : "false";

            return null;
        }

        return base.FormatValueAsString(value);
    }

    /// <summary>
    /// Renders the boolean options for the select element as child content.
    /// Includes a null option for <c>bool?</c> types.
    /// </summary>
    /// <param name="builder">The <see cref="RenderTreeBuilder"/> used to build the option elements.</param>
    /// <remarks>
    /// <para>
    /// This method generates the appropriate option elements based on the type parameter:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>For <c>bool?</c> types:</strong> Renders three options - null (empty value), true, and false</description></item>
    /// <item><description><strong>For <c>bool</c> types:</strong> Renders two options - true and false</description></item>
    /// </list>
    /// <para>
    /// Each option element includes:
    /// </para>
    /// <list type="bullet">
    /// <item><description>A "value" attribute with the appropriate string representation ("", "true", or "false")</description></item>
    /// <item><description>Display text using the corresponding label properties (<see cref="NullLabel"/>, <see cref="TrueLabel"/>, <see cref="FalseLabel"/>)</description></item>
    /// </list>
    /// <para>
    /// This method is automatically assigned to the <see cref="Microsoft.AspNetCore.Components.Forms.InputSelect{TValue}.ChildContent"/> 
    /// property during <see cref="OnParametersSet"/> if no custom child content is provided.
    /// </para>
    /// </remarks>
    protected void RenderBooleanOptions(RenderTreeBuilder builder)
    {
        if (typeof(TValue) == typeof(bool?))
        {
            builder.OpenElement(0, "option");
            builder.AddAttribute(1, "value", "");
            builder.AddContent(2, NullLabel);
            builder.CloseElement();
        }

        builder.OpenElement(3, "option");
        builder.AddAttribute(4, "value", "true");
        builder.AddContent(5, TrueLabel);
        builder.CloseElement();

        builder.OpenElement(6, "option");
        builder.AddAttribute(7, "value", "false");
        builder.AddContent(8, FalseLabel);
        builder.CloseElement();
    }
}
