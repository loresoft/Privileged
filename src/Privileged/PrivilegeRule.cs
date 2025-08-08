namespace Privileged;

/// <summary>
/// Represents a privilege authorization rule used to determine access rights
/// based on a subject, action, and optional qualifiers. The rule can either allow or deny access.
/// </summary>
public readonly record struct PrivilegeRule
{
    /// <summary>
    /// The action to match for this rule (e.g., "Read", "Write", "Delete").
    /// </summary>
    public string Action { get; init; }

    /// <summary>
    /// The subject to match for this rule (e.g., "Document", "User", etc.).
    /// This typically refers to the resource or entity being accessed.
    /// </summary>
    public string Subject { get; init; }

    /// <summary>
    /// An optional list of qualifiers that provide additional context for the rule
    /// (e.g., fields, specific tags, tenant IDs, or ownership constraints). If <c>null</c> or empty,
    /// the rule applies regardless of qualifier.
    /// </summary>
    public IReadOnlyCollection<string>? Qualifiers { get; init; }

    /// <summary>
    /// A value indicating whether this rule denies access. If <c>true</c>, the rule explicitly denies the action.
    /// If <c>false</c> or <c>null</c>, the rule allows the action.
    /// </summary>
    public bool? Denied { get; init; }

    /// <summary>
    /// Computes a hash code for the <see cref="PrivilegeAlias"/> based on its properties.
    /// </summary>
    /// <returns>A hash code representing the current state of the alias.</returns>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        hash.Add(Action);
        hash.Add(Subject);

        if (Qualifiers != null)
        {
            foreach (var qualifier in Qualifiers)
                hash.Add(qualifier);
        }

        hash.Add(Denied);

        return hash.ToHashCode();
    }
};
