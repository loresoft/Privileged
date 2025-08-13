using Microsoft.AspNetCore.Builder;

using Privileged.Authorization;

namespace Privileged.Endpoint;

/// <summary>
/// Provides extension methods for <see cref="IEndpointConventionBuilder" /> to require privileges.
/// </summary>
public static class EndpointConventionBuilderExtensions
{
    /// <summary>
    /// Adds a requirement for a specific privilege to the endpoint.
    /// </summary>
    /// <typeparam name="TBuilder">The type of the endpoint convention builder.</typeparam>
    /// <param name="builder">The endpoint convention builder.</param>
    /// <param name="action">The action required for the privilege.</param>
    /// <param name="subject">The subject of the privilege.</param>
    /// <param name="qualifier">An optional qualifier for the privilege.</param>
    /// <returns>The endpoint convention builder with the privilege requirement added.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder" /> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="action" /> or <paramref name="subject" /> is null or empty.</exception>
    public static TBuilder RequirePrivilege<TBuilder>(
        this TBuilder builder,
        string action,
        string subject,
        string? qualifier = null)
        where TBuilder : IEndpointConventionBuilder
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentException.ThrowIfNullOrEmpty(action);
        ArgumentException.ThrowIfNullOrEmpty(subject);

        var attribute = new PrivilegeAttribute(action, subject, qualifier);
        return builder.RequireAuthorization(attribute);
    }
}
