using Privileged;
using Privileged.Components;

namespace Sample.Application.Client.Services;

public class PrivilegeContextProvider : IPrivilegeContextProvider
{
    private PrivilegeContext? _cached;

    public ValueTask<PrivilegeContext> GetContextAsync()
    {
        if (_cached is not null)
            return ValueTask.FromResult(_cached);

        // Simulate different privilege sets. This could be swapped based on a fake user id.
        var builder = new PrivilegeBuilder()
            // Aliases
            .Alias("Crud", new[] { "create", "read", "update", "delete" }, PrivilegeMatch.Action)
            .Alias("PublicFields", new[] { "Title", "Summary" }, PrivilegeMatch.Qualifier)
            .Alias("SensitiveFields", new[] { "Cost", "InternalNotes" }, PrivilegeMatch.Qualifier)

            // Global allowances
            .Allow("read", PrivilegeSubjects.All)                 // read everything
            .Allow("update", "Product", new[] { "Title", "Summary" }) // update selected product fields
            .Allow("create", "Product")
            .Allow("read", "Order")

            // Forbid some specifics
            .Forbid("delete", "Product")
            .Forbid("update", "Product", new[] { "Cost" })      // override broader rules
            .Forbid("read", "Product", new[] { "InternalNotes" });

        _cached = builder.Build();
        return ValueTask.FromResult(_cached);
    }
}
