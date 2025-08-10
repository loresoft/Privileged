using Microsoft.AspNetCore.Authorization;

namespace Privileged.Authorization;

/// <summary>
/// Represents an authorization requirement that specifies the privilege needed to access a resource.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeRequirement"/> class implements <see cref="IAuthorizationRequirement"/>
/// and encapsulates the three core components of privilege-based authorization:
/// action, subject, and an optional qualifier.
/// </para>
/// <para>
/// This requirement is evaluated by the <see cref="PrivilegeRequirementHandler"/> which checks
/// if the current user's privilege context allows the specified action on the given subject
/// with the optional qualifier constraint.
/// </para>
/// </remarks>
/// <seealso cref="PrivilegeRequirementHandler"/>
/// <seealso cref="PrivilegeAttribute"/>
/// <seealso cref="PrivilegePolicyProvider"/>
public class PrivilegeRequirement : IAuthorizationRequirement
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeRequirement"/> class.
    /// </summary>
    /// <param name="action">The action that the user wants to perform (e.g., "read", "write", "delete").</param>
    /// <param name="subject">The subject (resource/entity) that the action will be performed on (e.g., "Post", "User").</param>
    /// <param name="qualifier">An optional qualifier that provides additional context for the privilege check (e.g., field names, scopes).</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is empty or whitespace.</exception>
    public PrivilegeRequirement(string action, string subject, string? qualifier = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        Action = action;
        Subject = subject;
        Qualifier = qualifier;
    }

    /// <summary>
    /// Gets the action that the user wants to perform.
    /// </summary>
    /// <value>
    /// A string representing the action (e.g., "read", "write", "delete", "create", "update").
    /// This value cannot be <c>null</c> or empty.
    /// </value>
    /// <remarks>
    /// Actions typically represent CRUD operations or business-specific operations.
    /// Common examples include "read", "write", "delete", "create", "update", "publish", "approve".
    /// </remarks>
    public string Action { get; }

    /// <summary>
    /// Gets the subject (resource/entity) that the action will be performed on.
    /// </summary>
    /// <value>
    /// A string representing the subject (e.g., "Post", "User", "Document", "Order").
    /// This value cannot be <c>null</c> or empty.
    /// </value>
    /// <remarks>
    /// Subjects typically represent domain entities or resources in your application.
    /// Examples include "Post", "User", "Document", "Order", "Product", "Comment".
    /// </remarks>
    public string Subject { get; }

    /// <summary>
    /// Gets the optional qualifier that provides additional context for the privilege check.
    /// </summary>
    /// <value>
    /// A string representing the qualifier, or <c>null</c> if no specific qualifier is required.
    /// Common qualifiers include field names, scopes, or other contextual information.
    /// </value>
    /// <remarks>
    /// Qualifiers enable fine-grained authorization by specifying additional constraints.
    /// For example, when checking if a user can update a "User" subject, the qualifier might
    /// specify which fields can be updated (e.g., "profile", "settings", "password").
    /// </remarks>
    public string? Qualifier { get; }
}
