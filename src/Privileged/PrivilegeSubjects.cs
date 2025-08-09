namespace Privileged;

/// <summary>
/// Provides default constants for privilege subjects used in authorization rules.
/// </summary>
public static class PrivilegeSubjects
{
    /// <summary>
    /// A special wildcard constant indicating that the rule applies to all subjects.
    /// When used in a privilege rule, it matches any subject value, providing a way
    /// to define global permissions that apply across all entities or resources.
    /// </summary>
    /// <value>The string "*" which serves as a wildcard for all subjects.</value>
    public const string All = "*";
}
