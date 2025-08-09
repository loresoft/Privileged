using Equatable.Attributes;

namespace Privileged;

/// <summary>
/// Represents a readonly model containing privilege rules and aliases used for authorization.
/// This struct encapsulates the data needed for privilege evaluation while maintaining immutability.
/// </summary>
[Equatable]
public partial record PrivilegeModel
{
    /// <summary>
    /// Gets or initializes the collection of privilege rules defined for this model.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeRule"/> instances representing the authorization rules.
    /// </value>
    [SequenceEquality]
    public required IReadOnlyList<PrivilegeRule> Rules { get; init; }

    /// <summary>
    /// Gets or initializes the collection of privilege aliases defined for this model.
    /// </summary>
    /// <value>
    /// A read-only collection of <see cref="PrivilegeAlias"/> instances used for rule aliasing or mapping.
    /// </value>
    [SequenceEquality]
    public IReadOnlyList<PrivilegeAlias> Aliases { get; init; } = [];
}
