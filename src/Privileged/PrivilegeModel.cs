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
    public required IReadOnlyList<PrivilegeRule> Rules { get; init; }

    /// <summary>
    /// Gets or initializes the collection of privilege aliases defined for this model.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeAlias"/> instances used for rule aliasing or mapping.
    /// </value>
    public IReadOnlyList<PrivilegeAlias> Aliases { get; init; } = [];


    public override int GetHashCode()
    {
        var hash = new HashCode();

        for (int r = 0; r < Rules.Count; r++)
            hash.Add(Rules[r]);

        for (int a = 0; a < Aliases.Count; a++)
            hash.Add(Aliases[a]);

        return hash.ToHashCode();
    }
}
