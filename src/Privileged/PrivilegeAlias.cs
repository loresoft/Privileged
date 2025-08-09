namespace Privileged;

/// <summary>
/// Represents an alias for one or more privilege values, allowing grouped or shorthand references
/// for subjects, actions, or qualifiers in privilege evaluations.
/// </summary>
public readonly record struct PrivilegeAlias()
{
    /// <summary>
    /// The alias name that will be used in rules or privilege checks (e.g., "modify").
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// A list of actual values that this alias expands to (e.g., ["read", "create", "update"]).
    /// </summary>
    public required IReadOnlyList<string> Values { get; init; }

    /// <summary>
    /// The type of match this alias applies to (e.g., <see cref="PrivilegeMatch.Subject"/> for subjects,
    /// <see cref="PrivilegeMatch.Action"/> for actions, or <see cref="PrivilegeMatch.Qualifier"/> for qualifiers).
    /// Defaults to <see cref="PrivilegeMatch.Action"/>.
    /// </summary>
    public PrivilegeMatch Type { get; init; } = PrivilegeMatch.Action;


    /// <summary>
    /// Computes a hash code for the <see cref="PrivilegeAlias"/> based on its properties.
    /// </summary>
    /// <returns>A hash code representing the current state of the alias.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Alias);

        foreach (var value in Values)
            hash.Add(value);

        hash.Add(Type);
        return hash.ToHashCode();
    }
}
