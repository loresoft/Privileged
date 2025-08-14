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
/// <list type="number">
/// <item>If the privilege context is <c>null</c> (loading): <see cref="Loading"/> template is rendered.</item>
/// <item>If the privilege context is loaded and <see cref="Loaded"/> is specified: <see cref="Loaded"/> template is rendered.</item>
/// <item>If the privilege context is loaded and <see cref="Loaded"/> is not specified: <see cref="ChildContent"/> is rendered.</item>
/// </list>
/// </para>
/// <para>
/// The component provides the loaded <see cref="PrivilegeContext"/> as a cascading value to all child components,
/// enabling privilege-aware components throughout the component tree.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="IPrivilegeContextProvider"/>
/// <seealso cref="AuthenticationStateProvider"/>
public class PrivilegeContextView : ComponentBase
{
    /// <summary>
    /// Gets or sets the provider for retrieving the privilege context.
    /// </summary>
    /// <value>
    /// An <see cref="IPrivilegeContextProvider"/> instance used to asynchronously load the privilege context.
    /// This service should be registered in the dependency injection container.
    /// </value>
    [Inject]
    protected IPrivilegeContextProvider PrivilegeContextProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the authentication state provider.
    /// </summary>
    /// <value>
    /// An <see cref="AuthenticationStateProvider"/> instance used to retrieve the current user's authentication state.
    /// This is passed to the privilege context provider to load user-specific privileges.
    /// </value>
    [Inject]
    protected AuthenticationStateProvider AuthenticationStateProvider { get; set; } = default!;

    /// <summary>
    /// Gets or sets the render fragment to display while the privilege context is loading.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to show during the loading state.
    /// If not specified, a default "Loading ..." message will be displayed.
    /// </value>
    [Parameter]
    public RenderFragment? Loading { get; set; } = builder => builder.AddMarkupContent(0, "Loading ...");

    /// <summary>
    /// Gets or sets the render fragment to display when the privilege context is loaded.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the content to show when the privilege context
    /// has been successfully loaded. This takes precedence over <see cref="ChildContent"/> when specified.
    /// </value>
    [Parameter]
    public RenderFragment? Loaded { get; set; }

    /// <summary>
    /// Gets or sets the child content to render within the privilege context.
    /// </summary>
    /// <value>
    /// A <see cref="RenderFragment"/> that defines the default content to render when the privilege context
    /// is loaded and <see cref="Loaded"/> is not specified.
    /// </value>
    [Parameter]
    public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Gets or sets the privilege context used for evaluating privilege rules.
    /// </summary>
    /// <value>
    /// The loaded <see cref="PrivilegeContext"/> instance, or <c>null</c> if the context is still loading.
    /// This value is set asynchronously during component initialization.
    /// </value>
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
    /// <list type="number">
    /// <item>Wraps all content in a <see cref="CascadingValue{TValue}"/> with the current <see cref="PrivilegeContext"/>.</item>
    /// <item>If <see cref="PrivilegeContext"/> is <c>null</c>: Renders <see cref="Loading"/> content.</item>
    /// <item>If <see cref="PrivilegeContext"/> is loaded and <see cref="Loaded"/> is specified: Renders <see cref="Loaded"/> content.</item>
    /// <item>If <see cref="PrivilegeContext"/> is loaded and <see cref="Loaded"/> is not specified: Renders <see cref="ChildContent"/>.</item>
    /// </list>
    /// </para>
    /// <para>
    /// The cascading value enables any child components that declare a <c>[CascadingParameter] PrivilegeContext</c>
    /// to automatically receive the loaded privilege context.
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
