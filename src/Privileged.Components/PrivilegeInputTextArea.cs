using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;

namespace Privileged.Components;

/// <summary>
/// A custom input text area component that integrates privilege-based access control.
/// </summary>
public class PrivilegeInputTextArea : InputTextArea
{
    /// <summary>
    /// Gets or sets the cascading privilege context used to evaluate access permissions.
    /// </summary>
    [CascadingParameter]
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Gets or sets the subject for privilege evaluation. Defaults to the model's type name if not specified.
    /// </summary>
    [Parameter]
    public string? Subject { get; set; }

    /// <summary>
    /// Gets or sets the field qualifier for privilege evaluation. Defaults to the input's name attribute if not specified.
    /// </summary>
    [Parameter]
    public string? Field { get; set; }

    /// <summary>
    /// Gets or sets the action name for read permissions. Defaults to "read".
    /// </summary>
    [Parameter]
    public string ReadAction { get; set; } = "read";

    /// <summary>
    /// Gets or sets the action name for update permissions. Defaults to "update".
    /// </summary>
    [Parameter]
    public string UpdateAction { get; set; } = "update";

    /// <summary>
    /// Gets a value indicating whether the user has read permission for the text area.
    /// </summary>
    protected bool HasReadPermission { get; set; }

    /// <summary>
    /// Gets a value indicating whether the user has update permission for the text area.
    /// </summary>
    protected bool HasUpdatePermission { get; set; }

    /// <summary>
    /// Called when the component's parameters are set. Evaluates permissions and adjusts attributes accordingly.
    /// </summary>
    /// <exception cref="InvalidOperationException">
    /// Thrown if the <see cref="PrivilegeContext"/> is not provided.
    /// </exception>
    protected override void OnParametersSet()
    {
        base.OnParametersSet();

        if (PrivilegeContext == null)
            throw new InvalidOperationException($"Component requires a cascading parameter of type {nameof(PrivilegeContext)}.");

        var subject = Subject ?? EditContext.Model.GetType().Name;
        var qualifier = Field ?? NameAttributeValue;

        HasReadPermission = PrivilegeContext.Allowed(ReadAction, subject, qualifier);
        HasUpdatePermission = PrivilegeContext.Allowed(UpdateAction, subject, qualifier);

        if (HasUpdatePermission)
            return;

        // If no update permission, set readonly or disabled attributes
        var attributes = AdditionalAttributes?.ToDictionary(kvp => kvp.Key, kvp => kvp.Value) ?? [];
        if (!HasUpdatePermission)
            attributes["readonly"] = true;

        AdditionalAttributes = attributes;
    }

    /// <summary>
    /// Builds the render tree for the component, rendering either the text area or a disabled password input based on permissions.
    /// </summary>
    /// <param name="builder">The <see cref="Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder"/> used to build the render tree.</param>
    protected override void BuildRenderTree(Microsoft.AspNetCore.Components.Rendering.RenderTreeBuilder builder)
    {
        // If the user has read permission, render the base InputTextArea component
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
