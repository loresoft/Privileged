using Microsoft.AspNetCore.Authorization;

namespace Privileged.Authorization;

/// <summary>
/// An authorization attribute that implements privilege-based authorization for ASP.NET Core.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeAttribute"/> extends <see cref="AuthorizeAttribute"/> to provide
/// declarative privilege-based authorization. When applied to controllers or actions, it creates
/// a dynamic authorization policy that requires the specified privilege.
/// </para>
/// <para>
/// The attribute works in conjunction with <see cref="PrivilegePolicyProvider"/> which dynamically
/// creates authorization policies based on the privilege requirements specified in the attribute.
/// These policies are then evaluated by <see cref="PrivilegeRequirementHandler"/>.
/// </para>
/// <para>
/// The policy name is automatically generated in the format:
/// <list type="bullet">
/// <item><description>Without qualifier: "Privilege:action:subject"</description></item>
/// <item><description>With qualifier: "Privilege:action:subject:qualifier"</description></item>
/// </list>
/// </para>
/// </remarks>
/// <example>
/// <para>Basic usage on controller actions:</para>
/// <code>
/// [ApiController]
/// [Route("api/[controller]")]
/// public class PostsController : ControllerBase
/// {
///     [HttpGet]
///     [Privilege("read", "Post")]
///     public IActionResult GetPosts()
///     {
///         // Only users with "read" privilege on "Post" can access this
///         return Ok();
///     }
///
///     [HttpPut("{id}")]
///     [Privilege("update", "Post")]
///     public IActionResult UpdatePost(int id)
///     {
///         // Only users with "update" privilege on "Post" can access this
///         return Ok();
///     }
///
///     [HttpPut("{id}/title")]
///     [Privilege("update", "Post", "title")]
///     public IActionResult UpdatePostTitle(int id)
///     {
///         // Only users with "update" privilege on "Post" for "title" field
///         return Ok();
///     }
/// }
/// </code>
/// </example>
/// <seealso cref="PrivilegeRequirement"/>
/// <seealso cref="PrivilegePolicyProvider"/>
/// <seealso cref="PrivilegeRequirementHandler"/>
public class PrivilegeAttribute : AuthorizeAttribute
{
    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeAttribute"/> class.
    /// </summary>
    /// <param name="action">The action that the user must be authorized to perform.</param>
    /// <param name="subject">The subject (resource/entity) that the action will be performed on.</param>
    /// <param name="qualifier">An optional qualifier that provides additional context for the privilege check.</param>
    /// <exception cref="ArgumentNullException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is <c>null</c>.</exception>
    /// <exception cref="ArgumentException">Thrown when <paramref name="action"/> or <paramref name="subject"/> is empty or whitespace.</exception>
    /// <remarks>
    /// The constructor automatically generates a policy name that encodes the privilege requirements.
    /// This policy name is used by the <see cref="PrivilegePolicyProvider"/> to create the appropriate
    /// authorization policy dynamically.
    /// </remarks>
    public PrivilegeAttribute(string action, string subject, string? qualifier = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        Action = action;
        Subject = subject;
        Qualifier = qualifier;

        // Create a dynamic policy name that encodes the privilege requirements
        // Use 3-part format when no qualifier, 4-part format when qualifier is provided
        Policy = string.IsNullOrEmpty(qualifier)
            ? $"Privilege:{action}:{subject}"
            : $"Privilege:{action}:{subject}:{qualifier}";
    }

    /// <summary>
    /// Gets the action that the user must be authorized to perform.
    /// </summary>
    /// <value>
    /// A string representing the required action (e.g., "read", "write", "delete").
    /// This value cannot be <c>null</c> or empty.
    /// </value>
    /// <remarks>
    /// Actions typically represent CRUD operations or business-specific operations that
    /// users can perform on resources in your application.
    /// </remarks>
    public string Action { get; }

    /// <summary>
    /// Gets the subject (resource/entity) that the action will be performed on.
    /// </summary>
    /// <value>
    /// A string representing the subject (e.g., "Post", "User", "Document").
    /// This value cannot be <c>null</c> or empty.
    /// </value>
    /// <remarks>
    /// Subjects typically represent domain entities or resources in your application
    /// that users can interact with.
    /// </remarks>
    public string Subject { get; }

    /// <summary>
    /// Gets the optional qualifier that provides additional context for the privilege check.
    /// </summary>
    /// <value>
    /// A string representing the qualifier, or <c>null</c> if no specific qualifier is required.
    /// </value>
    /// <remarks>
    /// Qualifiers enable fine-grained authorization by specifying additional constraints
    /// such as field names, scopes, or other contextual information.
    /// </remarks>
    public string? Qualifier { get; }
}
