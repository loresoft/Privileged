using Equatable.Attributes;

namespace Privileged;

/// <summary>
/// Represents an alias for one or more privilege values, allowing grouped or shorthand references
/// for subjects, actions, or qualifiers in privilege evaluations.
/// </summary>
[Equatable]
public partial record PrivilegeAlias
{
    /// <summary>
    /// The alias name that will be used in rules or privilege checks (e.g., "modify").
    /// </summary>
    public required string Alias { get; init; }

    /// <summary>
    /// A list of actual values that this alias expands to (e.g., ["read", "create", "update"]).
    /// </summary>
    [SequenceEquality]
    public required IReadOnlyList<string> Values { get; init; }

    /// <summary>
    /// The type of match this alias applies to (e.g., <see cref="PrivilegeMatch.Subject"/> for subjects,
    /// <see cref="PrivilegeMatch.Action"/> for actions, or <see cref="PrivilegeMatch.Qualifier"/> for qualifiers).
    /// Defaults to <see cref="PrivilegeMatch.Action"/>.
    /// </summary>
    public PrivilegeMatch Type { get; init; } = PrivilegeMatch.Action;
}
