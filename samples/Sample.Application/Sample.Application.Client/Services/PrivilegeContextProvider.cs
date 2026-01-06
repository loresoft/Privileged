using System.Security.Claims;

using Privileged;
using Privileged.Components;

namespace Sample.Application.Client.Services;

public class PrivilegeContextProvider : IPrivilegeContextProvider
{
    private PrivilegeContext? _cached;

    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        if (_cached is not null)
            return ValueTask.FromResult(_cached);

        // Simulate different privilege sets. This could be swapped based on a fake user id.
        var builder = new PrivilegeBuilder()
            // Aliases
            .Alias("Crud", ["create", "read", "update", "delete"], PrivilegeMatch.Action)
            .Alias("PublicFields", ["Title", "Summary"], PrivilegeMatch.Qualifier)
            .Alias("SensitiveFields", ["Cost", "InternalNotes"], PrivilegeMatch.Qualifier)

            // Global allowances
            .Allow("read", PrivilegeRule.Any)                 // read everything
            .Allow("update", "Product", ["Title", "Summary", "Password", "ReleaseDate"]) // update selected product fields
            .Allow("create", "Product")
            .Allow("read", "Order")

            // Forbid some specifics
            .Forbid("delete", "Product")
            .Forbid("update", "Product", ["Cost"])      // override broader rules
            .Forbid("read", "Product", ["InternalNotes"]);

        _cached = builder.Build();
        return ValueTask.FromResult(_cached);
    }
}
