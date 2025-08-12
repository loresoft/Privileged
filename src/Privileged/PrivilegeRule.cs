using Equatable.Attributes;

namespace Privileged;

/// <summary>
/// Represents a privilege authorization rule used to determine access rights
/// based on a subject, action, and optional qualifiers. The rule can either allow or deny access.
/// </summary>
[Equatable]
public partial record PrivilegeRule
{
    /// <summary>
    /// Represents a privilege rule that allows all actions and subjects.
    /// </summary>
    public static PrivilegeRule AllowAll { get; } = new()
    {
        Action = PrivilegeActions.All,
        Subject = PrivilegeSubjects.All
    };

    /// <summary>
    /// The action to match for this rule (e.g., "Read", "Write", "Delete").
    /// </summary>
    public required string Action { get; init; }

    /// <summary>
    /// The subject to match for this rule (e.g., "Document", "User", etc.).
    /// This typically refers to the resource or entity being accessed.
    /// </summary>
    public required string Subject { get; init; }

    /// <summary>
    /// An optional list of qualifiers that provide additional context for the rule
    /// (e.g., fields, specific tags, tenant IDs, or ownership constraints). If <c>null</c> or empty,
    /// the rule applies regardless of qualifier.
    /// </summary>
    [SequenceEquality]
    public IReadOnlyList<string>? Qualifiers { get; init; }

    /// <summary>
    /// A value indicating whether this rule denies access. If <c>true</c>, the rule explicitly denies the action.
    /// If <c>false</c> or <c>null</c>, the rule allows the action.
    /// </summary>
    public bool? Denied { get; init; }
};
