using System.Security.Claims;

using Privileged;

namespace Sample.MinimalApi;

public class SamplePrivilegeContextProvider : IPrivilegeContextProvider
{
    private readonly Lazy<PrivilegeContext> _privilegeContext = new(() =>
    {
        // Build a simple privilege context:
        //  - Allow read on Post
        //  - Allow update on Post only for title (qualifier)
        //  - Forbid delete on Post (to demonstrate a 403)
        return new PrivilegeBuilder()
            .Allow("read", "Post")
            .Allow("update", "Post", new[] { "title" })
            .Forbid("delete", "Post")
            .Build();
    });

    public ValueTask<PrivilegeContext> GetContextAsync(ClaimsPrincipal? claimsPrincipal = null)
    {
        return ValueTask.FromResult(_privilegeContext.Value);
    }
}
