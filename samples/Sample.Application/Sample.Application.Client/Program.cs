using Microsoft.AspNetCore.Components.WebAssembly.Hosting;

using Privileged.Components;

using Sample.Application.Client.Services;

namespace Sample.Application.Client;
internal class Program
{
    static async Task Main(string[] args)
    {
        var builder = WebAssemblyHostBuilder.CreateDefault(args);

        // register privilege context provider
        builder.Services.AddScoped<IPrivilegeContextProvider, PrivilegeContextProvider>();

        await builder.Build().RunAsync();
    }
}
