using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.DependencyInjection;

namespace Privileged.Authorization;

/// <summary>
/// Extension methods for configuring privilege-based authorization services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds privilege-based authorization services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers:
    /// <list type="bullet">
    /// <item><description>The <see cref="PrivilegeRequirementHandler"/> as a singleton authorization handler.</description></item>
    /// <item><description>The <see cref="PrivilegePolicyProvider"/> as a singleton policy provider.</description></item>
    /// </list>
    /// <para>
    /// You must also register an implementation of <see cref="IPrivilegeContextProvider"/> separately.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddPrivilegeAuthorization();
    /// builder.Services.AddScoped&lt;IPrivilegeContextProvider, YourPrivilegeContextProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddPrivilegeAuthorization(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        // Add authorization services if not already added
        services.AddAuthorizationCore();

        // Register the privilege requirement handler
        services.AddSingleton<IAuthorizationHandler, PrivilegeRequirementHandler>();

        // Register the custom policy provider
        services.AddSingleton<IAuthorizationPolicyProvider, PrivilegePolicyProvider>();

        return services;
    }

    /// <summary>
    /// Adds privilege-based authorization services with a custom <see cref="IPrivilegeContextProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The type of the privilege context provider implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="lifetime">The service lifetime for the privilege context provider. Defaults to <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddPrivilegeAuthorization&lt;MyPrivilegeContextProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddPrivilegeAuthorization<TProvider>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Transient)
        where TProvider : class, IPrivilegeContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPrivilegeAuthorization();

        // Register the privilege context provider
        services.Add(new ServiceDescriptor(typeof(IPrivilegeContextProvider), typeof(TProvider), lifetime));

        return services;
    }
}
