namespace Privileged.Components;

/// <summary>
/// Represents the privilege form state containing default privilege evaluation settings for form components.
/// </summary>
/// <param name="Subject">
/// The default subject for privilege evaluation. When specified, child components will use this
/// as their default subject unless they override it with their own Subject parameter.
/// </param>
/// <param name="ReadAction">
/// The default read action for privilege evaluation. When specified, child components will use this
/// as their default read action unless they override it with their own ReadAction parameter.
/// </param>
/// <param name="UpdateAction">
/// The default update action for privilege evaluation. When specified, child components will use this
/// as their default update action unless they override it with their own UpdateAction parameter.
/// </param>
/// <remarks>
/// <para>
/// This record is used to cascade default privilege settings from a <see cref="PrivilegeForm"/>
/// to its child privilege components. It provides a convenient way to set common privilege
/// parameters at the form level while allowing individual components to override specific values.
/// </para>
/// <para>
/// The record is immutable and is recreated whenever the parent form's parameters change,
/// ensuring that child components always receive the most current privilege settings.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// // Created automatically by PrivilegeForm with parameters:
/// var formState = new PrivilegeFormState("User", "view", "edit");
/// 
/// // Child components can access these values via cascading parameter:
/// // [CascadingParameter] protected PrivilegeFormState? PrivilegeFormState { get; set; }
/// </code>
/// </example>
/// <seealso cref="PrivilegeForm"/>
public record PrivilegeFormState(
    string? Subject,
    string? ReadAction,
    string? UpdateAction
);
