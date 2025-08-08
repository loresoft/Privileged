namespace Privileged;

/// <summary>
/// Represents a readonly model containing privilege rules and aliases used for authorization.
/// This struct encapsulates the data needed for privilege evaluation while maintaining immutability.
/// </summary>
public readonly record struct PrivilegeModel()
{
    /// <summary>
    /// Gets or initializes the collection of privilege rules defined for this model.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeRule"/> instances representing the authorization rules.
    /// </value>
    public required IReadOnlyCollection<PrivilegeRule> Rules { get; init; }

    /// <summary>
    /// Gets or initializes the collection of privilege aliases defined for this model.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeAlias"/> instances used for rule aliasing or mapping.
    /// </value>
    public IReadOnlyCollection<PrivilegeAlias> Aliases { get; init; } = [];
}
