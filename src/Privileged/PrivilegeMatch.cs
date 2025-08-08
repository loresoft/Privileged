namespace Privileged;

/// <summary>
/// Specifies the type of privilege component to match against aliases.
/// </summary>
public enum PrivilegeMatch
{
    /// <summary>
    /// Indicates a match on the privilege subject (e.g., resource or entity).
    /// </summary>
    Subject = 0,

    /// <summary>
    /// Indicates a match on the privilege action (e.g., read, write, delete).
    /// </summary>
    Action = 1,

    /// <summary>
    /// Indicates a match on the privilege qualifier (e.g., field, region).
    /// </summary>
    Qualifier = 2
}
