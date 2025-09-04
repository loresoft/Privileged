using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;

namespace Privileged.Components;

/// <summary>
/// A component that manages privilege context loading and provides cascading access to the privilege context.
/// This component asynchronously loads the privilege context using an <see cref="IPrivilegeContextProvider"/>
/// and displays appropriate content based on the loading state.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeContextView"/> component serves as a wrapper that handles the asynchronous
/// loading of privilege contexts and provides different rendering templates based on the loading state.
/// It automatically retrieves the authentication state and passes it to the privilege context provider.
/// </para>
/// <para>
/// Content rendering precedence:
/// </para>
/// <list type="number">
/// <item><description>If the privilege context is <c>null</c> (loading): <see cref="Loading"/> template is rendered.</description></item>
/// <item><description>If the privilege context is loaded and <see cref="Loaded"/> is specified: <see cref="Loaded"/> template is rendered.</description></item>
/// <item><description>If the privilege context is loaded and <see cref="Loaded"/> is not specified: <see cref="ChildContent"/> is rendered.</description></item>
/// </list>
/// <para>
/// The component provides the loaded <see cref="PrivilegeContext"/> as a cascading value to all child components,
/// enabling privilege-aware components throughout the component tree.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// &lt;PrivilegeContextView&gt;
///     &lt;Loading&gt;
///         &lt;div class="spinner"&gt;Loading permissions...&lt;/div&gt;
///     &lt;/Loading&gt;
///     &lt;Loaded&gt;
///         &lt;PrivilegeForm Model="userModel" Subject="User"&gt;
///             &lt;PrivilegeInputText @bind-Value="userModel.Name" /&gt;
///             &lt;PrivilegeInputCheckbox @bind-Value="userModel.IsActive" /&gt;
///         &lt;/PrivilegeForm&gt;
///     &lt;/Loaded&gt;
/// &lt;/PrivilegeContextView&gt;
/// 
/// &lt;!-- Simplified usage with ChildContent --&gt;
/// &lt;PrivilegeContextView&gt;
///     &lt;PrivilegeInputText @bind-Value="model.SecretField" Subject="Document" Field="Secret" /&gt;
/// &lt;/PrivilegeContextView&gt;
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="IPrivilegeContextProvider"/>
/// <seealso cref="AuthenticationStateProvider"/>
public class PrivilegeContextView : ComponentBase
{
    /// <summary>
    /// Gets or sets the provider for retrieving the privilege context.
    /// This service is automatically injected and must be registered in the dependency injection container.
    /// </summary>
    /// <value>
    /// An <see cref="IPrivilegeContextProvider"/> instance used to asynchronously load the privilege context.
    /// This service should be registered in the dependency injection container.
    /// </value>
    /// <remarks>
    /// The provider is responsible for loading user-specific privilege rules and aliases, typically from
    /// a database, cache, or external authorization service. The implementation should handle user identity
    /// and return appropriate privilege contexts based on the authenticated user's roles and permissions.
    /// </remarks>
    [Inject]
    protected IPrivilegeContextProvider PrivilegeContextProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the authentication state provider used to retrieve the current user's authentication information.
    /// This service is automatically injected from the Blazor authentication system.
    /// </summary>
    /// <value>
    /// An <see cref="AuthenticationStateProvider"/> instance used to retrieve the current user's authentication state.
    /// This is passed to the privilege context provider to load user-specific privileges.
    /// </value>
    /// <remarks>
    /// The authentication state contains the current user's <see cref="System.Security.Claims.ClaimsPrincipal"/>, 
    /// which is used by the privilege context provider to determine what privileges should be loaded.
    /// This enables user-specific privilege evaluation based on identity, roles, and claims.
    /// </remarks>
    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the render fragment to display while the privilege context is loading.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to show during the loading state.
    /// If not specified, a default "Loading ..." message will be displayed.
    /// </value>
    /// <remarks>
    /// This template is rendered when the component is initializing and waiting for the privilege context
    /// to be loaded from the <see cref="IPrivilegeContextProvider"/>. It provides an opportunity to show
    /// loading indicators, spinners, or placeholder content to improve user experience during async operations.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;Loading&gt;
    ///     &lt;div class="d-flex justify-content-center"&gt;
    ///         &lt;div class="spinner-border" role="status"&gt;
    ///             &lt;span class="sr-only"&gt;Loading permissions...&lt;/span&gt;
    ///         &lt;/div&gt;
    ///     &lt;/div&gt;
    /// &lt;/Loading&gt;
    /// </code>
    /// </example>
    [Parameter]
    public RenderFragment? Loading { get; set; } = builder => builder.AddMarkupContent(0, "Loading ...");

    /// <summary>
    /// Gets or sets the render fragment to display when the privilege context has been successfully loaded.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to show when the privilege context
    /// has been successfully loaded. This takes precedence over <see cref="ChildContent"/> when specified.
    /// </value>
    /// <remarks>
    /// This template is rendered after the privilege context has been successfully loaded and is available
    /// as a cascading value. When both <see cref="Loaded"/> and <see cref="ChildContent"/> are specified,
    /// the <see cref="Loaded"/> template takes precedence. This allows for different content structures
    /// based on the loading state.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;Loaded&gt;
    ///     &lt;div class="privilege-protected-content"&gt;
    ///         &lt;h2&gt;Secure Dashboard&lt;/h2&gt;
    ///         &lt;PrivilegeForm Model="dashboardModel"&gt;
    ///             &lt;!-- Privilege-aware form content --&gt;
    ///         &lt;/PrivilegeForm&gt;
    ///     &lt;/div&gt;
    /// &lt;/Loaded&gt;
    /// </code>
    /// </example>
    [Parameter]
    public RenderFragment? Loaded { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the privilege context.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the default content to render when the privilege context
    /// is loaded and <see cref="Loaded"/> is not specified.
    /// </value>
    /// <remarks>
    /// This is the default content template that is rendered when the privilege context is successfully loaded
    /// and no explicit <see cref="Loaded"/> template is provided. All content within this fragment will have
    /// access to the cascaded <see cref="PrivilegeContext"/> for privilege evaluation.
    /// </remarks>
    /// <example>
    /// <code>
    /// &lt;PrivilegeContextView&gt;
    ///     &lt;div class="user-profile"&gt;
    ///         &lt;PrivilegeInputText @bind-Value="user.Name" Subject="User" Field="Name" /&gt;
    ///         &lt;PrivilegeInputText @bind-Value="user.Email" Subject="User" Field="Email" /&gt;
    ///     &lt;/div&gt;
    /// &lt;/PrivilegeContextView&gt;
    /// </code>
    /// </example>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the privilege context used for evaluating privilege rules.
    /// </summary>
    /// <value>
    /// The loaded <see cref="PrivilegeContext"/> instance, or <c>null</c> if the context is still loading.
    /// This value is set asynchronously during component initialization.
    /// </value>
    /// <remarks>
    /// This property holds the privilege context that is loaded from the <see cref="IPrivilegeContextProvider"/>
    /// during component initialization. Once loaded, it is cascaded to all child components, enabling
    /// privilege-aware behavior throughout the component tree. The loading state can be determined by
    /// checking if this property is <c>null</c>.
    /// </remarks>
    protected PrivilegeContext? PrivilegeContext { get; set; }

    /// <summary>
    /// Builds the render tree for the component, displaying the appropriate content based on the loading state
    /// and wrapping the content in a cascading value that provides the privilege context to child components.
    /// </summary>
    /// <param name="builder">
    /// The <see cref="RenderTreeBuilder"/> used to construct the component's render tree.
    /// </param>
    /// <remarks>
    /// <para>
    /// The rendering logic follows this precedence:
    /// </para>
    /// <list type="number">
    /// <item><description>Wraps all content in a <see cref="CascadingValue{TValue}"/> with the current <see cref="PrivilegeContext"/>.</description></item>
    /// <item><description>If <see cref="PrivilegeContext"/> is <c>null</c>: Renders <see cref="Loading"/> content.</description></item>
    /// <item><description>If <see cref="PrivilegeContext"/> is loaded and <see cref="Loaded"/> is specified: Renders <see cref="Loaded"/> content.</description></item>
    /// <item><description>If <see cref="PrivilegeContext"/> is loaded and <see cref="Loaded"/> is not specified: Renders <see cref="ChildContent"/>.</description></item>
    /// </list>
    /// <para>
    /// The cascading value enables any child components that declare a <c>[CascadingParameter] PrivilegeContext</c>
    /// to automatically receive the loaded privilege context without requiring explicit parameter passing.
    /// </para>
    /// </remarks>
    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.OpenComponent<CascadingValue<PrivilegeContext?>>(0);
        builder.AddAttribute(1, nameof(CascadingValue<PrivilegeContext?>.Value), PrivilegeContext);
        builder.AddAttribute(2, nameof(CascadingValue<PrivilegeContext?>.ChildContent), BuildChildContent());
        builder.CloseComponent();
    }

    /// <summary>
    /// Asynchronously initializes the privilege context by retrieving it from the provider.
    /// This method is called once when the component is first initialized.
    /// </summary>
    /// <returns>A task that represents the asynchronous initialization operation.</returns>
    /// <remarks>
    /// <para>
    /// This method performs the following operations:
    /// </para>
    /// <list type="number">
    /// <item><description>Retrieves the current authentication state from the <see cref="AuthenticationStateProvider"/></description></item>
    /// <item><description>Extracts the <see cref="System.Security.Claims.ClaimsPrincipal"/> from the authentication state</description></item>
    /// <item><description>Calls the <see cref="IPrivilegeContextProvider"/> to load the privilege context for the authenticated user</description></item>
    /// <item><description>Sets the <see cref="PrivilegeContext"/> property with the loaded context</description></item>
    /// <item><description>Triggers a component re-render by calling <see cref="ComponentBase.StateHasChanged"/></description></item>
    /// </list>
    /// <para>
    /// The re-render after loading ensures that the UI transitions from the loading state to the loaded state,
    /// displaying the appropriate content template and making the privilege context available to child components.
    /// </para>
    /// </remarks>
    protected override async Task OnInitializedAsync()
    {
        var authenticationState = await AuthenticationStateProvider.GetAuthenticationStateAsync();
        PrivilegeContext = await PrivilegeContextProvider.GetContextAsync(authenticationState.User);
        StateHasChanged();
    }

    /// <summary>
    /// Builds the child content render fragment for the cascading value based on the current loading state.
    /// </summary>
    /// <returns>
    /// A <see cref="RenderFragment"/> that renders the appropriate content based on whether the
    /// <see cref="PrivilegeContext"/> is loaded and which templates are available.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method implements the content selection logic for the component:
    /// </para>
    /// <list type="bullet">
    /// <item><description><strong>Loading state:</strong> When <see cref="PrivilegeContext"/> is <c>null</c>, renders the <see cref="Loading"/> template</description></item>
    /// <item><description><strong>Loaded with explicit template:</strong> When <see cref="PrivilegeContext"/> is available and <see cref="Loaded"/> is specified, renders the <see cref="Loaded"/> template</description></item>
    /// <item><description><strong>Loaded with default content:</strong> When <see cref="PrivilegeContext"/> is available and no <see cref="Loaded"/> template is specified, renders the <see cref="ChildContent"/></description></item>
    /// </list>
    /// <para>
    /// The returned render fragment is executed within the context of the cascading value, ensuring that
    /// all rendered content has access to the privilege context for authorization decisions.
    /// </para>
    /// </remarks>
    private RenderFragment BuildChildContent()
    {
        return childBuilder =>
        {
            if (PrivilegeContext is null)
            {
                childBuilder.AddContent(0, Loading);
            }
            else if (Loaded is not null)
            {
                childBuilder.AddContent(1, Loaded);
            }
            else if (ChildContent is not null)
            {
                childBuilder.AddContent(2, ChildContent);
            }
        };
    }
}
