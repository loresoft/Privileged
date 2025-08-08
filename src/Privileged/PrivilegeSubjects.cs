namespace Privileged;

/// <summary>
/// Provides default constants for privilege subjects used in authorization rules.
/// </summary>
/// <remarks>
/// This class contains predefined constants that can be used when defining privilege rules
/// for subjects (entities or resources) in the authorization system. Subjects typically
/// represent business entities such as "User", "Post", "Project", or other domain objects.
/// </remarks>
/// <example>
/// The following example shows how to use <see cref="All"/> in privilege rules:
/// <code>
/// var context = new PrivilegeBuilder()
///     .Allow("read", PrivilegeSubjects.All)    // Allows reading any subject
///     .Allow("manage", "Post")                 // Allows managing Post entities
///     .Forbid("delete", PrivilegeSubjects.All) // Forbids deleting any subject
///     .Build();
///
/// bool canReadUser = context.Allowed("read", "User");     // true (matches All)
/// bool canReadPost = context.Allowed("read", "Post");     // true (matches All)
/// bool canDeletePost = context.Allowed("delete", "Post"); // false (forbidden by All rule)
/// </code>
/// </example>
/// <seealso cref="PrivilegeActions"/>
/// <seealso cref="PrivilegeBuilder"/>
/// <seealso cref="PrivilegeContext"/>
public static class PrivilegeSubjects
{
    /// <summary>
    /// A special wildcard constant indicating that the rule applies to all subjects.
    /// When used in a privilege rule, it matches any subject value, providing a way
    /// to define global permissions that apply across all entities or resources.
    /// </summary>
    /// <value>The string "*" which serves as a wildcard for all subjects.</value>
    /// <remarks>
    /// This constant is particularly useful for:
    /// <list type="bullet">
    /// <item><description>Creating global allow or forbid rules that apply to all subjects</description></item>
    /// <item><description>Defining fallback permissions when no specific subject rules match</description></item>
    /// <item><description>Implementing role-based permissions where certain roles have broad access</description></item>
    /// </list>
    /// When multiple rules exist, more specific subject rules will take precedence over wildcard rules.
    /// </remarks>
    /// <example>
    /// The following examples demonstrate various uses of the <see cref="All"/> constant:
    /// <code>
    /// // Allow reading any subject
    /// builder.Allow("read", PrivilegeSubjects.All);
    /// 
    /// // Forbid deletion of any subject (global restriction)
    /// builder.Forbid("delete", PrivilegeSubjects.All);
    /// 
    /// // Combined with specific rules
    /// builder.Allow("manage", PrivilegeSubjects.All)  // Allow managing anything
    ///        .Forbid("manage", "AdminSettings");      // Except AdminSettings
    /// </code>
    /// </example>
    public const string All = "*";
}
