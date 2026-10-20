using System.Security.Claims;

using Microsoft.AspNetCore.Components.Authorization;

namespace Privileged.Components.Tests;

public class PrivilegeServiceTests
{
    private static PrivilegeContext CreateContext()
        => new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("update", "Post")
            .Forbid("delete", "Post")
            .Build();

    [Fact]
    public async Task Allowed_Returns_Expected_Results()
    {
        var service = new PrivilegeService(new MockPrivilegeContextProvider(CreateContext()));

        Assert.True(await service.Allowed("read", "Post"));
        Assert.True(await service.Allowed("update", "Post"));
        Assert.False(await service.Allowed("delete", "Post"));
        Assert.False(await service.Allowed("read", "User"));
    }

    [Fact]
    public async Task Forbidden_Returns_Inverse_Of_Allowed()
    {
        var service = new PrivilegeService(new MockPrivilegeContextProvider(CreateContext()));

        Assert.False(await service.Forbidden("read", "Post"));
        Assert.True(await service.Forbidden("delete", "Post"));
    }

    [Fact]
    public async Task Allowed_Returns_False_For_Null_Action_Or_Subject()
    {
        var service = new PrivilegeService(new MockPrivilegeContextProvider(CreateContext()));

        Assert.False(await service.Allowed(null, "Post"));
        Assert.False(await service.Allowed("read", null));
        Assert.True(await service.Forbidden(null, "Post"));
    }

    [Fact]
    public async Task Allowed_Returns_False_When_Provider_Returns_Null()
    {
        var service = new PrivilegeService(new NullReturningProvider());

        Assert.False(await service.Allowed("read", "Post"));
    }

    [Fact]
    public async Task GetContextAsync_Returns_Empty_And_Caches_When_Provider_Returns_Null()
    {
        var provider = new CountingPrivilegeContextProvider(null!, TimeSpan.Zero);
        var service = new PrivilegeService(provider);

        var context = await service.GetContextAsync();
        await service.GetContextAsync();

        Assert.Same(PrivilegeContext.Empty, context);
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task Reloads_Context_When_Authentication_State_Changes()
    {
        var alice = CreateUser("alice");
        var bob = CreateUser("bob");

        var authProvider = new ChangeableAuthenticationStateProvider(alice);
        var provider = new PerUserPrivilegeContextProvider();
        var service = new PrivilegeService(provider, authProvider);

        Assert.True(await service.Allowed("update", "Post"));
        Assert.True(await service.Allowed("read", "Post"));
        Assert.Equal(1, provider.CallCount);

        authProvider.SetUser(bob);

        Assert.False(await service.Allowed("update", "Post"));
        Assert.True(await service.Allowed("read", "Post"));
        Assert.Equal(2, provider.CallCount);
        Assert.Same(bob, provider.LastPrincipal);
    }

    [Fact]
    public async Task Does_Not_Cache_Context_Loaded_Before_Authentication_State_Changed()
    {
        var alice = CreateUser("alice");
        var bob = CreateUser("bob");

        var authProvider = new ChangeableAuthenticationStateProvider(alice);
        var gate = new TaskCompletionSource();
        var provider = new PerUserPrivilegeContextProvider { Gate = gate };
        var service = new PrivilegeService(provider, authProvider);

        // start loading for alice, then switch users before the load completes
        var staleTask = service.Allowed("update", "Post");
        authProvider.SetUser(bob);

        gate.SetResult();
        Assert.True(await staleTask);

        // the stale context for alice must not have been cached
        Assert.False(await service.Allowed("update", "Post"));
        Assert.Same(bob, provider.LastPrincipal);
        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task Dispose_Unsubscribes_And_Prevents_Further_Use()
    {
        var authProvider = new ChangeableAuthenticationStateProvider(CreateUser("alice"));
        var service = new PrivilegeService(new PerUserPrivilegeContextProvider(), authProvider);

        await service.GetContextAsync();
        Assert.Equal(1, authProvider.SubscriberCount);

        service.Dispose();
        service.Dispose();

        Assert.Equal(0, authProvider.SubscriberCount);
        await Assert.ThrowsAsync<ObjectDisposedException>(() => service.Allowed("read", "Post"));
    }

    [Fact]
    public async Task Can_Be_Used_As_Permission_Resolver()
    {
        var service = new PrivilegeService(new MockPrivilegeContextProvider(CreateContext()));

        // matches the Func<Task<bool>> PermissionResolver parameter used by Arbiter components
        Func<Task<bool>> allowedResolver = () => service.Allowed("update", "Post");
        Func<Task<bool>> deniedResolver = () => service.Allowed("delete", "Post");

        Assert.True(await allowedResolver());
        Assert.False(await deniedResolver());
    }

    [Fact]
    public async Task GetContextAsync_Loads_Context_Once()
    {
        var provider = new CountingPrivilegeContextProvider(CreateContext(), TimeSpan.FromMilliseconds(50));
        var service = new PrivilegeService(provider);

        var tasks = Enumerable.Range(0, 10)
            .Select(_ => service.Allowed("read", "Post"))
            .ToArray();

        var results = await Task.WhenAll(tasks);

        Assert.All(results, Assert.True);
        Assert.True(await service.Allowed("update", "Post"));
        Assert.Same(await service.GetContextAsync(), await service.GetContextAsync());
        Assert.Equal(1, provider.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_Retries_After_Failure()
    {
        var provider = new FailingOncePrivilegeContextProvider(CreateContext());
        var service = new PrivilegeService(provider);

        await Assert.ThrowsAsync<InvalidOperationException>(() => service.Allowed("read", "Post"));

        Assert.True(await service.Allowed("read", "Post"));
        Assert.Equal(2, provider.CallCount);
    }

    [Fact]
    public async Task GetContextAsync_Passes_Authenticated_User_To_Provider()
    {
        var user = new ClaimsPrincipal(new ClaimsIdentity([new Claim(ClaimTypes.Name, "TestUser")], "Test"));
        var provider = new CountingPrivilegeContextProvider(CreateContext(), TimeSpan.Zero);
        var service = new PrivilegeService(provider, new MockAuthenticationStateProvider(user));

        await service.GetContextAsync();

        Assert.Same(user, provider.LastPrincipal);
    }

    private static ClaimsPrincipal CreateUser(string name)
        => new(new ClaimsIdentity([new Claim(ClaimTypes.Name, name)], "Test"));

    private sealed class ChangeableAuthenticationStateProvider(ClaimsPrincipal user) : AuthenticationStateProvider
    {
        private static readonly System.Reflection.FieldInfo EventField = typeof(AuthenticationStateProvider)
            .GetField(nameof(AuthenticationStateChanged), System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!;

        private ClaimsPrincipal _user = user;

        public int SubscriberCount
            => (EventField.GetValue(this) as Delegate)?.GetInvocationList().Length ?? 0;

        public override Task<AuthenticationState> GetAuthenticationStateAsync()
            => Task.FromResult(new AuthenticationState(_user));

        public void SetUser(ClaimsPrincipal user)
        {
            _user = user;
            NotifyAuthenticationStateChanged(GetAuthenticationStateAsync());
        }
    }

    private sealed class PerUserPrivilegeContextProvider : IPrivilegeContextProvider
    {
        public TaskCompletionSource? Gate { get; set; }

        public int CallCount { get; private set; }

        public ClaimsPrincipal? LastPrincipal { get; private set; }

        public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
        {
            CallCount++;
            LastPrincipal = claimsPrincipal;

            var gate = Gate;
            Gate = null;
            if (gate is not null)
                await gate.Task;

            var builder = new PrivilegeBuilder().Allow("read", "Post");
            if (claimsPrincipal?.Identity?.Name == "alice")
                builder.Allow("update", "Post");

            return builder.Build();
        }
    }

    private sealed class CountingPrivilegeContextProvider(PrivilegeContext context, TimeSpan delay) : IPrivilegeContextProvider
    {
        private int _callCount;

        public int CallCount => _callCount;

        public ClaimsPrincipal? LastPrincipal { get; private set; }

        public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
        {
            Interlocked.Increment(ref _callCount);
            LastPrincipal = claimsPrincipal;

            if (delay > TimeSpan.Zero)
                await Task.Delay(delay);

            return context;
        }
    }

    private sealed class FailingOncePrivilegeContextProvider(PrivilegeContext context) : IPrivilegeContextProvider
    {
        public int CallCount { get; private set; }

        public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
        {
            CallCount++;
            await Task.Yield();

            if (CallCount == 1)
                throw new InvalidOperationException("Load failed");

            return context;
        }
    }
}
