namespace Privileged;

/// <summary>
/// Provides default constants for privilege actions used in authorization rules.
/// </summary>
public static class PrivilegeActions
{
    /// <summary>
    /// A special keyword indicating that the rule applies to all actions.
    /// When used in a rule, it matches any action value.
    /// </summary>
    public const string All = "*";
}
