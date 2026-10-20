using Microsoft.AspNetCore.Components.Authorization;

namespace Privileged.Components;

/// <summary>
/// Provides privilege evaluation for the current user using a lazily loaded <see cref="PrivilegeContext"/>.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeContext"/> is loaded from the <see cref="IPrivilegeContextProvider"/> the first time
/// it is needed and cached. Concurrent callers share a single load operation, and subsequent calls complete
/// synchronously using the cached context.
/// </para>
/// <para>
/// When an <see cref="AuthenticationStateProvider"/> is available, the current user's
/// <see cref="System.Security.Claims.ClaimsPrincipal"/> is passed to the provider so user-specific privileges
/// can be loaded. The cached context is discarded whenever
/// <see cref="AuthenticationStateProvider.AuthenticationStateChanged"/> is raised, so the next check loads the
/// context for the new user.
/// </para>
/// <para>
/// Register this service with a scoped lifetime, using
/// <see cref="ServiceCollectionExtensions.AddPrivilegeComponents(Microsoft.Extensions.DependencyInjection.IServiceCollection)"/>,
/// so each user or circuit gets its own instance.
/// </para>
/// </remarks>
public class PrivilegeService : IDisposable
{
    private readonly IPrivilegeContextProvider _privilegeContextProvider;
    private readonly AuthenticationStateProvider? _authenticationStateProvider;

#if NET9_0_OR_GREATER
    private readonly Lock _initializeLock = new();
#else
    private readonly object _initializeLock = new();
#endif

    private Task<PrivilegeContext>? _initializeTask;
    private volatile PrivilegeContext? _privilegeContext;
    private int _version;
    private bool _disposed;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeService"/> class.
    /// </summary>
    /// <param name="privilegeContextProvider">The provider used to load the <see cref="PrivilegeContext"/>.</param>
    /// <param name="authenticationStateProvider">
    /// An optional authentication state provider used to retrieve the current user and to detect user changes.
    /// When <c>null</c>, the privilege context is loaded without a user.
    /// </param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="privilegeContextProvider"/> is <c>null</c>.</exception>
    public PrivilegeService(
        IPrivilegeContextProvider privilegeContextProvider,
        AuthenticationStateProvider? authenticationStateProvider = null)
    {
        _privilegeContextProvider = privilegeContextProvider ?? throw new ArgumentNullException(nameof(privilegeContextProvider));
        _authenticationStateProvider = authenticationStateProvider;

        _authenticationStateProvider?.AuthenticationStateChanged += OnAuthenticationStateChanged;
    }


    /// <summary>
    /// Determines whether the specified action is allowed for the given subject and optional qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "read", "write").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the action is allowed for the subject and qualifier; otherwise, <c>false</c>.
    /// Returns <c>false</c> when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// The privilege context is loaded on first use via <see cref="GetContextAsync"/>.
    /// </para>
    /// <para>
    /// Returns a <see cref="Task{TResult}"/> so it can be used directly as a <c>Func&lt;Task&lt;bool&gt;&gt;</c>
    /// permission resolver, for example <c>PermissionResolver="() =&gt; PrivilegeService.Allowed(&quot;update&quot;, &quot;Post&quot;)"</c>.
    /// Once the context is loaded, the call completes synchronously using cached <see cref="Task{TResult}"/> results.
    /// </para>
    /// <para>
    /// Exceptions thrown while loading the context are not caught; they propagate to the caller. The failed load
    /// is not cached, so the next call retries it.
    /// </para>
    /// </remarks>
    /// <exception cref="Exception">
    /// Any exception thrown by the <see cref="AuthenticationStateProvider"/> or <see cref="IPrivilegeContextProvider"/>
    /// while loading the context.
    /// </exception>
    /// <seealso cref="PrivilegeContext.Allowed(string?, string?, string?)"/>
    public async Task<bool> Allowed(string? action, string? subject, string? qualifier = null)
    {
        if (action is null || subject is null)
            return false;

        var privilegeContext = await GetContextAsync();

        return privilegeContext.Allowed(action, subject, qualifier);
    }

    /// <summary>
    /// Determines whether the specified action is forbidden for the given subject and optional qualifier.
    /// </summary>
    /// <param name="action">The action to authorize (e.g., "delete", "update").</param>
    /// <param name="subject">The subject to authorize (e.g., a resource or entity).</param>
    /// <param name="qualifier">An optional qualifier that further scopes the privilege (e.g., a field or scope).</param>
    /// <returns>
    /// <c>true</c> if the action is not allowed for the subject and qualifier; otherwise, <c>false</c>.
    /// Returns <c>true</c> when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This is the logical inverse of <see cref="Allowed(string?, string?, string?)"/>.
    /// The privilege context is loaded on first use via <see cref="GetContextAsync"/>.
    /// </para>
    /// <para>
    /// Exceptions thrown while loading the context are not caught; they propagate to the caller rather than
    /// being treated as forbidden. The failed load is not cached, so the next call retries it.
    /// </para>
    /// </remarks>
    /// <exception cref="Exception">
    /// Any exception thrown by the <see cref="AuthenticationStateProvider"/> or <see cref="IPrivilegeContextProvider"/>
    /// while loading the context.
    /// </exception>
    public async Task<bool> Forbidden(string? action, string? subject, string? qualifier = null)
        => !await Allowed(action, subject, qualifier);

    /// <summary>
    /// Gets the <see cref="PrivilegeContext"/> for the current user, loading it on first access.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask{TResult}"/> that resolves to the loaded <see cref="PrivilegeContext"/>.
    /// Never resolves to <c>null</c>; if the provider returns <c>null</c>, <see cref="PrivilegeContext.Empty"/> is used.
    /// </returns>
    /// <remarks>
    /// <para>
    /// On first access, the current user is retrieved from the <see cref="AuthenticationStateProvider"/> (if available)
    /// and the context is loaded from the <see cref="IPrivilegeContextProvider"/>. The result is cached until the
    /// authentication state changes or this service is disposed.
    /// </para>
    /// <para>
    /// Concurrent callers share the same load operation. Once loaded, this method completes synchronously
    /// without allocating. If loading faults or is canceled, the exception propagates to the caller and the
    /// next call retries the load.
    /// </para>
    /// </remarks>
    /// <exception cref="ObjectDisposedException">Thrown when the service has been disposed.</exception>
    /// <exception cref="Exception">
    /// Any exception thrown by the <see cref="AuthenticationStateProvider"/> or <see cref="IPrivilegeContextProvider"/>
    /// while loading the context.
    /// </exception>
    public ValueTask<PrivilegeContext> GetContextAsync()
    {
        var privilegeContext = _privilegeContext;
        if (privilegeContext is not null)
            return new ValueTask<PrivilegeContext>(privilegeContext);

        lock (_initializeLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);

            privilegeContext = _privilegeContext;
            if (privilegeContext is not null)
                return new ValueTask<PrivilegeContext>(privilegeContext);

            if (_initializeTask?.IsFaulted != false || _initializeTask.IsCanceled)
                _initializeTask = LoadContextAsync(_version);

            return new ValueTask<PrivilegeContext>(_initializeTask);
        }
    }


    /// <summary>
    /// Releases the resources used by the <see cref="PrivilegeService"/>, unsubscribing from
    /// authentication state changes and discarding the cached context.
    /// </summary>
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    /// <summary>
    /// Releases the resources used by the <see cref="PrivilegeService"/>.
    /// </summary>
    /// <param name="disposing"><c>true</c> when called from <see cref="Dispose()"/>; otherwise, <c>false</c>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (!disposing)
            return;

        lock (_initializeLock)
        {
            if (_disposed)
                return;

            _disposed = true;
            ResetContext();
        }

        if (_authenticationStateProvider is not null)
            _authenticationStateProvider.AuthenticationStateChanged -= OnAuthenticationStateChanged;
    }


    private async Task<PrivilegeContext> LoadContextAsync(int version)
    {
        var authenticationState = _authenticationStateProvider != null
            ? await _authenticationStateProvider.GetAuthenticationStateAsync()
            : null;

        var privilegeContext = await _privilegeContextProvider
            .GetContextAsync(authenticationState?.User);

        // the provider contract is non-null, but fall back to deny-all so callers never see null
        privilegeContext ??= PrivilegeContext.Empty;

        lock (_initializeLock)
        {
            // don't cache a context loaded for a user that has since changed
            if (version == _version && !_disposed)
                _privilegeContext = privilegeContext;
        }

        return privilegeContext;
    }

    private void OnAuthenticationStateChanged(Task<AuthenticationState> task)
    {
        lock (_initializeLock)
            ResetContext();
    }

    private void ResetContext()
    {
        _version++;
        _privilegeContext = null;
        _initializeTask = null;
    }
}
