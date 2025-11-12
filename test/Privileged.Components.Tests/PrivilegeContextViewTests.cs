using System.Security.Claims;

using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Authorization;
using Microsoft.AspNetCore.Components.Rendering;
using Microsoft.Extensions.DependencyInjection;

namespace Privileged.Components.Tests;

public class PrivilegeContextViewTests : BunitContext
{
    public PrivilegeContextViewTests()
    {
        // Provide a mock AuthenticationStateProvider with a default authenticated user
        var mockUser = new ClaimsPrincipal(new ClaimsIdentity(new[]
        {
            new Claim(ClaimTypes.Name, "TestUser"),
            new Claim(ClaimTypes.Role, "Administrator")
        }, "TestAuthentication"));

        var mockAuthStateProvider = new MockAuthenticationStateProvider(mockUser);
        Services.AddSingleton<AuthenticationStateProvider>(mockAuthStateProvider);
    }

    [Fact]
    public void Displays_Loading_Content_When_Context_Is_Null()
    {
        var mockProvider = new MockPrivilegeContextProvider(null);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<p>Loading privileges...</p>"))
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<p>Content loaded</p>"))
        );

        cut.Find("p").MarkupMatches("<p>Loading privileges...</p>");
    }

    [Fact]
    public void Displays_Default_Loading_Content_When_No_Loading_Template_Provided()
    {
        var mockProvider = new MockPrivilegeContextProvider(null);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<p>Content loaded</p>"))
        );

        cut.Markup.Should().Contain("Loading ...");
    }

    [Fact]
    public void Displays_Loaded_Content_When_Context_Is_Available()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var mockProvider = new MockPrivilegeContextProvider(context);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<p>Loading privileges...</p>"))
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<p>Privileges loaded successfully</p>"))
        );

        cut.Find("p").MarkupMatches("<p>Privileges loaded successfully</p>");
    }

    [Fact]
    public void Displays_ChildContent_When_Context_Available_And_No_Loaded_Template()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var mockProvider = new MockPrivilegeContextProvider(context);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<p>Loading privileges...</p>"))
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<p>Child content displayed</p>"))
        );

        cut.Find("p").MarkupMatches("<p>Child content displayed</p>");
    }

    [Fact]
    public void Prefers_Loaded_Over_ChildContent_When_Both_Provided()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Build();

        var mockProvider = new MockPrivilegeContextProvider(context);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<p>Loading privileges...</p>"))
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<p>Loaded content</p>"))
            .Add(p => p.ChildContent, builder => builder.AddMarkupContent(0, "<p>Child content</p>"))
        );

        cut.Find("p").MarkupMatches("<p>Loaded content</p>");
        cut.Markup.Should().NotContain("Child content");
    }

    [Fact]
    public void Provides_PrivilegeContext_As_Cascading_Value()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("update", "User")
            .Build();

        var mockProvider = new MockPrivilegeContextProvider(context);
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loaded, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<TestPrivilegeConsumer>(0);
                builder.CloseComponent();
            })
        );

        var consumer = cut.FindComponent<TestPrivilegeConsumer>();
        consumer.Instance.ReceivedContext.Should().NotBeNull();
        consumer.Instance.ReceivedContext!.Allowed("read", "Post").Should().BeTrue();
        consumer.Instance.ReceivedContext!.Allowed("update", "User").Should().BeTrue();
        consumer.Instance.ReceivedContext!.Allowed("delete", "Post").Should().BeFalse();
    }

    [Fact]
    public void Handles_Async_Provider_Correctly()
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "Document")
            .Build();

        var asyncProvider = new AsyncMockPrivilegeContextProvider(context, TimeSpan.FromMilliseconds(50));
        Services.AddSingleton<IPrivilegeContextProvider>(asyncProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<div class='loading'>Loading...</div>"))
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<div class='loaded'>Ready!</div>"))
        );

        cut.Find(".loading").Should().NotBeNull();

        cut.WaitForAssertion(() => cut.Find(".loaded").Should().NotBeNull(), TimeSpan.FromSeconds(1));
    }

    [Fact]
    public void Works_With_Real_Provider_Implementation()
    {
        var realProvider = new TestPrivilegeContextProvider();
        Services.AddSingleton<IPrivilegeContextProvider>(realProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<span>Loading real context...</span>"))
            .Add(p => p.Loaded, (RenderTreeBuilder builder) =>
            {
                builder.OpenComponent<TestPrivilegeConsumer>(0);
                builder.CloseComponent();
            })
        );

        var consumer = cut.FindComponent<TestPrivilegeConsumer>();
        consumer.Instance.ReceivedContext.Should().NotBeNull();
        consumer.Instance.ReceivedContext!.Allowed("read", "TestEntity").Should().BeTrue();
        consumer.Instance.ReceivedContext!.Allowed("write", "TestEntity").Should().BeTrue();
        consumer.Instance.ReceivedContext!.Allowed("delete", "TestEntity").Should().BeFalse();
    }

    [Fact]
    public void Handles_Null_Provider_Gracefully()
    {
        var mockProvider = new NullReturningProvider();
        Services.AddSingleton<IPrivilegeContextProvider>(mockProvider);

        var cut = Render<PrivilegeContextView>(parameters => parameters
            .Add(p => p.Loading, builder => builder.AddMarkupContent(0, "<div class='loading'>Loading...</div>"))
            .Add(p => p.Loaded, builder => builder.AddMarkupContent(0, "<div class='loaded'>Ready!</div>"))
        );

        cut.Find(".loading").Should().NotBeNull();
        cut.Markup.Should().NotContain("Ready!");
    }
}

// Test helper classes
internal class MockPrivilegeContextProvider : IPrivilegeContextProvider
{
    private readonly PrivilegeContext? _context;

    public MockPrivilegeContextProvider(PrivilegeContext? context)
    {
        _context = context;
    }

    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        return _context != null
            ? ValueTask.FromResult(_context)
            : ValueTask.FromResult<PrivilegeContext>(null!);
    }
}

internal class AsyncMockPrivilegeContextProvider : IPrivilegeContextProvider
{
    private readonly PrivilegeContext _context;
    private readonly TimeSpan _delay;

    public AsyncMockPrivilegeContextProvider(PrivilegeContext context, TimeSpan delay)
    {
        _context = context;
        _delay = delay;
    }

    public async ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        await Task.Delay(_delay);
        return _context;
    }
}

internal class TestPrivilegeContextProvider : IPrivilegeContextProvider
{
    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        var context = new PrivilegeBuilder()
            .Allow("read", "TestEntity")
            .Allow("write", "TestEntity")
            .Forbid("delete", "TestEntity")
            .Build();

        return ValueTask.FromResult(context);
    }
}

internal class NullReturningProvider : IPrivilegeContextProvider
{
    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        return ValueTask.FromResult<PrivilegeContext>(null!);
    }
}

internal class TestPrivilegeConsumer : ComponentBase
{
    [CascadingParameter]
    public PrivilegeContext? ReceivedContext { get; set; }

    protected override void BuildRenderTree(RenderTreeBuilder builder)
    {
        builder.AddContent(0, $"Received context: {(ReceivedContext != null ? "Yes" : "No")}");
    }
}

internal class MockAuthenticationStateProvider : AuthenticationStateProvider
{
    private readonly ClaimsPrincipal _user;

    public MockAuthenticationStateProvider(ClaimsPrincipal user)
    {
        _user = user;
    }

    public override Task<AuthenticationState> GetAuthenticationStateAsync()
    {
        return Task.FromResult(new AuthenticationState(_user));
    }
}
