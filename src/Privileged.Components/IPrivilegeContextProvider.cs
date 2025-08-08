namespace Privileged.Components;

/// <summary>
/// Provides a mechanism to retrieve a <see cref="PrivilegeContext"/> for evaluating privilege rules.
/// </summary>
public interface IPrivilegeContextProvider
{
    /// <summary>
    /// Asynchronously retrieves the current <see cref="PrivilegeContext"/>.
    /// </summary>
    /// <returns>
    /// A <see cref="ValueTask"/> that represents the asynchronous operation. The task result contains the <see cref="PrivilegeContext"/>.
    /// </returns>
    ValueTask<PrivilegeContext> GetContextAsync();
}

