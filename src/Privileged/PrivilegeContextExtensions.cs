using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Privileged;

/// <summary>
/// Provides extension methods for <see cref="PrivilegeContext"/> to perform bulk authorization checks
/// across multiple subjects with convenient methods for common scenarios.
/// </summary>
/// <remarks>
/// <para>
/// These extension methods enable efficient authorization checking when you need to verify
/// permissions across multiple subjects with the same action. They provide a convenient
/// way to check if any, all, or none of the specified subjects are allowed for a given action.
/// </para>
/// </remarks>
/// <example>
/// <code>
/// var context = new PrivilegeBuilder()
///     .Allow("read", "Post")
///     .Allow("read", "Comment")
///     .Forbid("read", "User")
///     .Build();
///
/// // Check if user can read any of the specified subjects
/// bool canReadAny = context.Any("read", "Post", "User", "Comment"); // true
///
/// // Check if user can read all of the specified subjects
/// bool canReadAll = context.All("read", "Post", "Comment"); // true
/// bool canReadAllIncludingUser = context.All("read", "Post", "User", "Comment"); // false
///
/// // Check if user cannot read any of the specified subjects
/// bool canReadNone = context.None("read", "User", "Admin"); // true
/// </code>
/// </example>
public static class PrivilegeContextExtensions
{
    /// <summary>
    /// Determines whether the specified action is allowed for any of the given subjects.
    /// </summary>
    /// <param name="context">The privilege context to check against.</param>
    /// <param name="action">The action to authorize (e.g., "read", "write", "delete").</param>
    /// <param name="subjects">The subjects to check permissions for (e.g., resource names or entity types).</param>
    /// <returns>
    /// <c>true</c> if the specified action is allowed for at least one of the subjects; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="subjects"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="action"/> is <c>null</c>, empty, or contains only whitespace characters.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses short-circuit evaluation and returns <c>true</c> immediately when the first
    /// allowed subject is found, making it efficient for checking permissions across many subjects.
    /// </para>
    /// <para>
    /// Each subject is checked individually using the <see cref="PrivilegeContext.Allowed(string?, string?, string?)"/>
    /// method with no qualifier specified.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = new PrivilegeBuilder()
    ///     .Allow("read", "Post")
    ///     .Forbid("read", "User")
    ///     .Build();
    ///
    /// // Returns true because "read" is allowed on "Post"
    /// bool canReadAny = context.Any("read", "Post", "User", "Comment");
    ///
    /// // Returns false because "delete" is not allowed on any subject
    /// bool canDeleteAny = context.Any("delete", "Post", "User", "Comment");
    /// </code>
    /// </example>
    public static bool Any(this PrivilegeContext context, string action, params IEnumerable<string> subjects)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(subjects);

        foreach (var subject in subjects)
        {
            if (context.Allowed(action, subject))
                return true;
        }

        return false;
    }

    /// <summary>
    /// Determines whether the specified action is allowed for all of the given subjects.
    /// </summary>
    /// <param name="context">The privilege context to check against.</param>
    /// <param name="action">The action to authorize (e.g., "read", "write", "delete").</param>
    /// <param name="subjects">The subjects to check permissions for (e.g., resource names or entity types).</param>
    /// <returns>
    /// <c>true</c> if the specified action is allowed for all of the subjects; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="subjects"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="action"/> is <c>null</c>, empty, or contains only whitespace characters.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses short-circuit evaluation and returns <c>false</c> immediately when the first
    /// forbidden subject is found, making it efficient for checking permissions across many subjects.
    /// </para>
    /// <para>
    /// Each subject is checked individually using the <see cref="PrivilegeContext.Allowed(string?, string?, string?)"/>
    /// method with no qualifier specified.
    /// </para>
    /// <para>
    /// If the subjects collection is empty, this method returns <c>true</c> (vacuous truth).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = new PrivilegeBuilder()
    ///     .Allow("read", "Post")
    ///     .Allow("read", "Comment")
    ///     .Forbid("read", "User")
    ///     .Build();
    ///
    /// // Returns true because "read" is allowed on both "Post" and "Comment"
    /// bool canReadAll = context.All("read", "Post", "Comment");
    ///
    /// // Returns false because "read" is forbidden on "User"
    /// bool canReadAllIncludingUser = context.All("read", "Post", "User", "Comment");
    /// </code>
    /// </example>
    public static bool All(this PrivilegeContext context, string action, params IEnumerable<string> subjects)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(subjects);

        foreach (var subject in subjects)
        {
            if (!context.Allowed(action, subject))
                return false;
        }

        return true;
    }

    /// <summary>
    /// Determines whether the specified action is not allowed (forbidden or not explicitly allowed) for any of the given subjects.
    /// </summary>
    /// <param name="context">The privilege context to check against.</param>
    /// <param name="action">The action to authorize (e.g., "read", "write", "delete").</param>
    /// <param name="subjects">The subjects to check permissions for (e.g., resource names or entity types).</param>
    /// <returns>
    /// <c>true</c> if the specified action is not allowed for any of the subjects; otherwise, <c>false</c>.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown when <paramref name="context"/> or <paramref name="subjects"/> is <c>null</c>.
    /// </exception>
    /// <exception cref="ArgumentException">
    /// Thrown when <paramref name="action"/> is <c>null</c>, empty, or contains only whitespace characters.
    /// </exception>
    /// <remarks>
    /// <para>
    /// This method uses short-circuit evaluation and returns <c>false</c> immediately when the first
    /// allowed subject is found, making it efficient for checking permissions across many subjects.
    /// </para>
    /// <para>
    /// Each subject is checked individually using the <see cref="PrivilegeContext.Allowed(string?, string?, string?)"/>
    /// method with no qualifier specified.
    /// </para>
    /// <para>
    /// This method is the logical inverse of <see cref="Any"/> - it returns <c>true</c> only when
    /// none of the subjects allow the specified action.
    /// </para>
    /// <para>
    /// If the subjects collection is empty, this method returns <c>true</c> (vacuous truth).
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var context = new PrivilegeBuilder()
    ///     .Allow("read", "Post")
    ///     .Forbid("read", "User")
    ///     .Build();
    ///
    /// // Returns false because "read" is allowed on "Post"
    /// bool canReadNone = context.None("read", "Post", "User");
    ///
    /// // Returns true because "delete" is not allowed on any subject
    /// bool canDeleteNone = context.None("delete", "Post", "User", "Comment");
    ///
    /// // Returns true because neither "User" nor "Admin" allow "read"
    /// bool canReadNoneRestricted = context.None("read", "User", "Admin");
    /// </code>
    /// </example>
    public static bool None(this PrivilegeContext context, string action, params IEnumerable<string> subjects)
    {
        ArgumentNullException.ThrowIfNull(context);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentNullException.ThrowIfNull(subjects);

        foreach (var subject in subjects)
        {
            if (context.Allowed(action, subject))
                return false;
        }

        return true;
    }
}
