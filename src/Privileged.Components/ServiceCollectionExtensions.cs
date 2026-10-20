using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Privileged.Components;

/// <summary>
/// Extension methods for configuring privilege Blazor component services.
/// </summary>
public static class ServiceCollectionExtensions
{
    /// <summary>
    /// Adds privilege Blazor component services to the specified <see cref="IServiceCollection"/>.
    /// </summary>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <remarks>
    /// This method registers:
    /// <list type="bullet">
    /// <descriptor><description>The <see cref="PrivilegeService"/> as a scoped service.</description></descriptor>
    /// </list>
    /// <para>
    /// <see cref="PrivilegeService"/> caches the privilege context for its lifetime, so it is registered as scoped
    /// to give each user or circuit its own instance. You must also register an implementation of
    /// <see cref="IPrivilegeContextProvider"/> separately.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// builder.Services.AddPrivilegeComponents();
    /// builder.Services.AddScoped&lt;IPrivilegeContextProvider, YourPrivilegeContextProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddPrivilegeComponents(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.TryAddScoped<PrivilegeService>();

        return services;
    }

    /// <summary>
    /// Adds privilege Blazor component services with a custom <see cref="IPrivilegeContextProvider"/>.
    /// </summary>
    /// <typeparam name="TProvider">The type of the privilege context provider implementation.</typeparam>
    /// <param name="services">The <see cref="IServiceCollection"/> to add services to.</param>
    /// <param name="lifetime">The service lifetime for the privilege context provider. Defaults to <see cref="ServiceLifetime.Scoped"/>.</param>
    /// <returns>The <see cref="IServiceCollection"/> so that additional calls can be chained.</returns>
    /// <example>
    /// <code>
    /// builder.Services.AddPrivilegeComponents&lt;MyPrivilegeContextProvider&gt;();
    /// </code>
    /// </example>
    public static IServiceCollection AddPrivilegeComponents<TProvider>(
        this IServiceCollection services,
        ServiceLifetime lifetime = ServiceLifetime.Scoped)
        where TProvider : class, IPrivilegeContextProvider
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddPrivilegeComponents();

        ServiceDescriptor descriptor = new(
            serviceType: typeof(IPrivilegeContextProvider),
            implementationType: typeof(TProvider),
            lifetime: lifetime);

        services.Add(descriptor);

        return services;
    }
}
