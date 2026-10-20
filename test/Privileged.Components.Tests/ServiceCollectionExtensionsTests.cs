using Microsoft.Extensions.DependencyInjection;

namespace Privileged.Components.Tests;

public class ServiceCollectionExtensionsTests
{
    [Fact]
    public void AddPrivilegeComponents_Registers_PrivilegeService_As_Scoped()
    {
        var services = new ServiceCollection();

        services.AddPrivilegeComponents();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(PrivilegeService));
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public void AddPrivilegeComponents_Does_Not_Register_Duplicate_PrivilegeService()
    {
        var services = new ServiceCollection();

        services.AddPrivilegeComponents();
        services.AddPrivilegeComponents();

        Assert.Single(services, d => d.ServiceType == typeof(PrivilegeService));
    }

    [Fact]
    public void AddPrivilegeComponents_With_Provider_Registers_Provider()
    {
        var services = new ServiceCollection();

        services.AddPrivilegeComponents<TestPrivilegeContextProvider>();

        var descriptor = Assert.Single(services, d => d.ServiceType == typeof(IPrivilegeContextProvider));
        Assert.Equal(typeof(TestPrivilegeContextProvider), descriptor.ImplementationType);
        Assert.Equal(ServiceLifetime.Scoped, descriptor.Lifetime);
    }

    [Fact]
    public async Task PrivilegeService_Resolves_Per_Scope()
    {
        var services = new ServiceCollection();
        services.AddPrivilegeComponents<TestPrivilegeContextProvider>();

        using var provider = services.BuildServiceProvider();

        using var scope1 = provider.CreateScope();
        using var scope2 = provider.CreateScope();

        var service1 = scope1.ServiceProvider.GetRequiredService<PrivilegeService>();
        var service1Again = scope1.ServiceProvider.GetRequiredService<PrivilegeService>();
        var service2 = scope2.ServiceProvider.GetRequiredService<PrivilegeService>();

        Assert.Same(service1, service1Again);
        Assert.NotSame(service1, service2);

        Assert.True(await service1.Allowed("read", "TestEntity"));
        Assert.False(await service1.Allowed("delete", "TestEntity"));
    }
}
