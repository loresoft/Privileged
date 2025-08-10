using System.Security.Claims;

namespace Privileged;

/// <summary>
/// Provides a mechanism to retrieve a <see cref="PrivilegeContext"/> for evaluating privilege rules.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="IPrivilegeContextProvider"/> interface defines a contract for supplying privilege contexts
/// to authorization components. Implementations of this interface are responsible for obtaining privilege
/// data from various sources such as databases, caches, external services, or static configurations.
/// </para>
/// </remarks>
/// <example>
/// <para>Simple static implementation:</para>
/// <code>
/// public class StaticPrivilegeContextProvider : IPrivilegeContextProvider
/// {
///     public ValueTask&lt;PrivilegeContext&gt; GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
///     {
///         var context = new PrivilegeContext(
///             new List&lt;PrivilegeRule&gt;
///             {
///                 new PrivilegeRule { Action = "read", Subject = "Post" },
///                 new PrivilegeRule { Action = "write", Subject = "Post", Qualifiers = new List&lt;string&gt; { "title", "content" } }
///             }
///         );
///         return ValueTask.FromResult(context);
///     }
/// }
/// </code>
///
/// <para>Database-backed implementation with caching:</para>
/// <code>
/// public class DatabasePrivilegeContextProvider : IPrivilegeContextProvider
/// {
///     private readonly IUserService _userService;
///     private readonly IMemoryCache _cache;
///
///     public DatabasePrivilegeContextProvider(IUserService userService, IMemoryCache cache)
///     {
///         _userService = userService;
///         _cache = cache;
///     }
///
///     public async ValueTask&lt;PrivilegeContext&gt; GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
///     {
///         var userId = claimsPrincipal?.FindFirst(ClaimTypes.NameIdentifier)?.Value;
///         if (string.IsNullOrEmpty(userId))
///             return PrivilegeContext.Empty;
///
///         var cacheKey = $"privileges:{userId}";
///
///         if (_cache.TryGetValue(cacheKey, out PrivilegeContext? cachedContext))
///             return cachedContext;
///
///         var privileges = await _userService.GetUserPrivilegesAsync(userId);
///         var context = new PrivilegeContext(privileges);
///
///         _cache.Set(cacheKey, context, TimeSpan.FromMinutes(10));
///         return context;
///     }
/// }
/// </code>
///
/// <para>Registration in dependency injection:</para>
/// <code>
/// services.AddScoped&lt;IPrivilegeContextProvider, DatabasePrivilegeContextProvider&gt;();
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
public interface IPrivilegeContextProvider
{
    /// <summary>
    /// Asynchronously retrieves the current <see cref="PrivilegeContext"/> for privilege evaluation.
    /// </summary>
    /// <returns>
    /// A <see cref="Task{TResult}"/> that represents the asynchronous operation.
    /// The task result contains the <see cref="PrivilegeContext"/> that will be used for authorization checks.
    /// </returns>
    ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null);
}

