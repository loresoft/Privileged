namespace Privileged;

/// <summary>
/// A fluent builder for constructing a <see cref="PrivilegeContext"/> using declarative rule and alias definitions.
/// </summary>
/// <remarks>
/// <para>
/// The <see cref="PrivilegeBuilder"/> provides a fluent interface for defining authorization rules
/// that determine what actions users can perform on specific subjects (resources/entities) with optional qualifiers.
/// Rules can either allow or deny access, and aliases can be defined to group related values for reuse.
/// </para>
/// <para>
/// Rules are evaluated in the order they are defined, with more specific rules taking precedence over general ones.
/// Forbid rules override allow rules when both match the same criteria.
/// </para>
/// <para>
/// For bulk operations with multiple actions or subjects, see the extension methods in <see cref="PrivilegeBuilderExtensions"/>.
/// </para>
/// </remarks>
/// <example>
/// <para>Basic usage with simple rules:</para>
/// <code>
/// var context = new PrivilegeBuilder()
///     .Allow("read", "Post")                    // Allow reading posts
///     .Allow("write", "User")                   // Allow writing users
///     .Forbid("delete", "User")                 // Forbid deleting users
///     .Build();
///
/// bool canRead = context.Allowed("read", "Post");         // true
/// bool canDelete = context.Forbidden("delete", "User");   // true
/// </code>
///
/// <para>Advanced usage with aliases and qualifiers:</para>
/// <code>
/// var context = new PrivilegeBuilder()
///     .Alias("Manage", new[] { "Create", "Update", "Delete" }, PrivilegeMatch.Action)
///     .Allow("Manage", "Project")                                 // Allows all actions defined in the "Manage" alias
///     .Allow("Read", "User")                                      // Allows reading User
///     .Allow("Update", "User", new[] { "Profile", "Settings" })   // Allows updating User's Profile and Settings
///     .Forbid("Delete", "User")                                   // Forbids deleting User
///     .Build();
///
/// bool canCreateProject = context.Allowed("Create", "Project");           // true
/// bool canReadUser = context.Allowed("Read", "User");                     // true
/// bool canUpdateProfile = context.Allowed("Update", "User", "Profile");   // true
/// bool canUpdatePassword = context.Allowed("Update", "User", "Password"); // false
/// bool canDeleteUser = context.Allowed("Delete", "User");                 // false
/// </code>
///
/// <para>Using wildcard constants for global rules:</para>
/// <code>
/// var context = new PrivilegeBuilder()
///     .Allow("read", PrivilegeRule.All)     // Allow reading any subject
///     .Allow(PrivilegeRule.All, "Post")      // Allow any action on posts
///     .Forbid("delete", PrivilegeRule.All)  // Forbid deleting anything
///     .Build();
/// </code>
/// </example>
/// <seealso cref="PrivilegeContext"/>
/// <seealso cref="PrivilegeBuilderExtensions"/>
/// <seealso cref="PrivilegeRule"/>
/// <seealso cref="PrivilegeRule"/>
public class PrivilegeBuilder
{
    private readonly List<PrivilegeRule> _rules = [];
    private readonly List<PrivilegeAlias> _aliases = [];
    private StringComparer _stringComparer = StringComparer.InvariantCultureIgnoreCase;

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeBuilder"/> class with empty rules and aliases.
    /// </summary>
    public PrivilegeBuilder()
    {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeBuilder"/> class with the specified rules, aliases, and string comparer.
    /// </summary>
    /// <param name="rules">
    /// The initial collection of privilege rules to include in the builder.
    /// Cannot be null.
    /// </param>
    /// <param name="aliases">
    /// An optional collection of privilege aliases to include in the builder.
    /// If null, no aliases are added initially.
    /// </param>
    /// <param name="stringComparer">
    /// An optional string comparer for matching privilege names.
    /// If null, defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.
    /// </param>
    public PrivilegeBuilder(IEnumerable<PrivilegeRule> rules, IEnumerable<PrivilegeAlias>? aliases = null, StringComparer? stringComparer = null)
    {
        ArgumentNullException.ThrowIfNull(rules);

        _rules = rules.ToList();
        _aliases = aliases?.ToList() ?? [];
        _stringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrivilegeBuilder"/> class from an existing <see cref="PrivilegeModel"/>.
    /// </summary>
    /// <param name="model">
    /// The privilege model containing rules and aliases to initialize the builder with.
    /// Cannot be null.
    /// </param>
    /// <param name="stringComparer">
    /// An optional string comparer for matching privilege names.
    /// If null, defaults to <see cref="StringComparer.InvariantCultureIgnoreCase"/>.
    /// </param>
    public PrivilegeBuilder(PrivilegeModel model, StringComparer? stringComparer = null)
    {
        ArgumentNullException.ThrowIfNull(model);

        _rules = model.Rules.ToList();
        _aliases = model.Aliases.ToList();
        _stringComparer = stringComparer ?? StringComparer.InvariantCultureIgnoreCase;
    }

    /// <summary>
    /// Sets the <see cref="StringComparer"/> to use for matching privilege names.
    /// </summary>
    /// <param name="comparer">
    /// The <see cref="StringComparer"/> to use for case-insensitive or culture-specific comparisons.
    /// If <c>null</c>, an <see cref="ArgumentNullException"/> is thrown.
    /// </param>
    /// <returns>
    /// The current <see cref="PrivilegeBuilder"/> instance for method chaining.
    /// </returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="comparer"/> is <c>null</c>.
    /// </exception>
    /// <remarks>
    /// <para>
    /// The comparer affects how privilege names (actions, subjects, and qualifiers) are matched.
    /// For example, using <see cref="StringComparer.InvariantCultureIgnoreCase"/> allows case-insensitive matching.
    /// </para>
    /// </remarks>
    public PrivilegeBuilder Comparer(StringComparer comparer)
    {
        ArgumentNullException.ThrowIfNull(comparer);

        _stringComparer = comparer;
        return this;
    }

    /// <summary>
    /// Creates a rule that allows the specified action on the given subject and optional qualifiers.
    /// </summary>
    /// <param name="action">
    /// The action to allow (e.g., "read", "create", "update").
    /// Can be a wildcard using <see cref="PrivilegeRule.All"/> to match any action.
    /// </param>
    /// <param name="subject">
    /// The subject to allow (e.g., a resource or entity name like "Post", "User").
    /// Can be a wildcard using <see cref="PrivilegeRule.All"/> to match any subject.
    /// </param>
    /// <param name="qualifiers">
    /// An optional collection of qualifiers that further scope the rule (e.g., field names, tags, or regions).
    /// If null or empty, the rule applies regardless of qualifier. When specified, the rule only applies
    /// when the qualifier exactly matches one of the provided values.
    /// </param>
    /// <returns>The current <see cref="PrivilegeBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="action"/> or <paramref name="subject"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Allow rules grant explicit permission for the specified action on the subject.
    /// If multiple rules apply to the same criteria, forbid rules take precedence over allow rules.
    /// </para>
    /// <para>
    /// For bulk operations with multiple actions or subjects, use the extension methods in
    /// <see cref="PrivilegeBuilderExtensions"/> such as
    /// <c>Allow(IEnumerable&lt;string&gt; actions, string subject)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = new PrivilegeBuilder()
    ///     .Allow("read", "Post")                                  // Basic rule
    ///     .Allow("edit", "Post", new[] { "title", "content" })    // Rule with qualifiers
    ///     .Allow(PrivilegeRule.All, "Comment")                 // Wildcard action
    ///     .Allow("manage", PrivilegeRule.All);                // Wildcard subject
    /// </code>
    /// </example>
    /// <seealso cref="Forbid(string, string, IEnumerable{string}?)"/>
    /// <seealso cref="PrivilegeBuilderExtensions.Allow(PrivilegeBuilder, IEnumerable{string}, string, IEnumerable{string}?)"/>
    public PrivilegeBuilder Allow(string action, string subject, IEnumerable<string>? qualifiers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var qualifierSet = qualifiers?.ToList();
        var rule = new PrivilegeRule
        {
            Action = action,
            Subject = subject,
            Qualifiers = qualifierSet
        };

        _rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Creates a rule that forbids the specified action on the given subject and optional qualifiers.
    /// </summary>
    /// <param name="action">
    /// The action to forbid (e.g., "delete", "update", "publish").
    /// Can be a wildcard using <see cref="PrivilegeRule.All"/> to forbid any action.
    /// </param>
    /// <param name="subject">
    /// The subject to forbid (e.g., a resource or entity name like "Post", "User").
    /// Can be a wildcard using <see cref="PrivilegeRule.All"/> to forbid on any subject.
    /// </param>
    /// <param name="qualifiers">
    /// An optional collection of qualifiers that further scope the rule (e.g., field names, tags, or regions).
    /// If null or empty, the rule applies regardless of qualifier. When specified, the rule only applies
    /// when the qualifier exactly matches one of the provided values.
    /// </param>
    /// <returns>The current <see cref="PrivilegeBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">
    /// Thrown if <paramref name="action"/> or <paramref name="subject"/> is null or whitespace.
    /// </exception>
    /// <remarks>
    /// <para>
    /// Forbid rules explicitly deny permission for the specified action on the subject.
    /// Forbid rules take precedence over allow rules when both match the same criteria,
    /// providing a way to create exceptions to broader allow rules.
    /// </para>
    /// <para>
    /// For bulk operations with multiple actions or subjects, use the extension methods in
    /// <see cref="PrivilegeBuilderExtensions"/> such as
    /// <c>Forbid(IEnumerable&lt;string&gt; actions, string subject)</c>.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = new PrivilegeBuilder()
    ///     .Allow(PrivilegeRule.All, "Post")             // Allow all actions on posts
    ///     .Forbid("delete", "Post")                        // Except deletion
    ///     .Forbid("edit", "Post", ["sensitive_data"])      // Forbid editing sensitive fields
    ///     .Forbid(PrivilegeRule.All, "AdminSettings");  // Forbid all actions on admin settings
    /// </code>
    /// </example>
    /// <seealso cref="Allow(string, string, IEnumerable{string}?)"/>
    /// <seealso cref="PrivilegeBuilderExtensions.Forbid(PrivilegeBuilder, IEnumerable{string}, string, IEnumerable{string}?)"/>
    public PrivilegeBuilder Forbid(string action, string subject, IEnumerable<string>? qualifiers = null)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(action);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        var qualifierSet = qualifiers?.ToList();
        var rule = new PrivilegeRule
        {
            Action = action,
            Subject = subject,
            Qualifiers = qualifierSet,
            Denied = true
        };

        _rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Defines an alias for a group of values that can be referenced by name in subsequent rules.
    /// </summary>
    /// <param name="alias">
    /// The name of the alias (e.g., "AdminActions", "PublicResources").
    /// This name will be used in place of individual values in rules.
    /// </param>
    /// <param name="values">
    /// The collection of values that this alias represents (e.g., ["Create", "Update", "Delete"] for actions,
    /// or ["User", "Post", "Comment"] for subjects). All values in the collection will be matched
    /// when the alias is used in a rule.
    /// </param>
    /// <param name="type">
    /// The privilege component type that this alias applies to:
    /// <see cref="PrivilegeMatch.Action"/> for actions,
    /// <see cref="PrivilegeMatch.Subject"/> for subjects, or
    /// <see cref="PrivilegeMatch.Qualifier"/> for qualifiers.
    /// </param>
    /// <returns>The current <see cref="PrivilegeBuilder"/> instance for method chaining.</returns>
    /// <exception cref="ArgumentException">Thrown if <paramref name="alias"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="values"/> is null.</exception>
    /// <remarks>
    /// <para>
    /// Aliases provide a convenient way to group related values and reference them by a single name,
    /// reducing duplication and making rules more maintainable. When a rule uses an alias name,
    /// it is expanded to match any of the values defined in the alias.
    /// </para>
    /// <para>
    /// Aliases must be defined before they are used in rules. The same alias name can be reused
    /// for different privilege types (action, subject, qualifier) without conflict.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = new PrivilegeBuilder()
    ///     // Define aliases for common groupings
    ///     .Alias("CRUD", ["Create", "Read", "Update", "Delete"], PrivilegeMatch.Action)
    ///     .Alias("ContentTypes", ["Post", "Article", "Comment"], PrivilegeMatch.Subject)
    ///     .Alias("PublicFields", ["title", "summary", "author"], PrivilegeMatch.Qualifier)
    ///
    ///     // Use aliases in rules
    ///     .Allow("CRUD", "ContentTypes")                    // Expands to all CRUD actions on all content types
    ///     .Allow("read", "Post", ["PublicFields"])          // Allow reading only public fields
    ///     .Forbid("Delete", "ContentTypes");                // Forbid deletion of any content type
    /// </code>
    /// </example>
    /// <seealso cref="PrivilegeMatch"/>
    /// <seealso cref="PrivilegeAlias"/>
    public PrivilegeBuilder Alias(string alias, IEnumerable<string> values, PrivilegeMatch type)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(alias, nameof(alias));
        ArgumentNullException.ThrowIfNull(values);

        var aliasValues = values.ToList();
        var privilegeAlias = new PrivilegeAlias
        {
            Alias = alias,
            Values = aliasValues,
            Type = type
        };
        _aliases.Add(privilegeAlias);

        return this;
    }

    /// <summary>
    /// Merges the current rules and aliases with those from the provided <see cref="PrivilegeModel" />.
    /// Removes duplicates based on action, subject, and qualifiers for rules, and alias name for aliases.
    /// </summary>
    /// <param name="model">The <see cref="PrivilegeModel" /> containing rules and aliases to merge.</param>
    /// <returns>The current <see cref="PrivilegeBuilder" /> instance for method chaining.</returns>
    public PrivilegeBuilder Merge(PrivilegeModel model)
    {
        ArgumentNullException.ThrowIfNull(model);

        // Merge rules, removing duplicates
        var existingRules = new HashSet<PrivilegeRule>(_rules);
        foreach (var rule in model.Rules)
        {
            if (!existingRules.Contains(rule))
                _rules.Add(rule);
        }

        // Merge aliases, removing duplicates by alias name
        var existingAliases = new HashSet<PrivilegeAlias>(_aliases);
        foreach (var alias in model.Aliases)
        {
            if (!existingAliases.Contains(alias))
                _aliases.Add(alias);
        }

        return this;
    }

    /// <summary>
    /// Builds and returns a new <see cref="PrivilegeContext"/> instance containing all configured rules and aliases.
    /// </summary>
    /// <returns>
    /// A <see cref="PrivilegeContext"/> populated with the rules and aliases defined via
    /// <see cref="Allow"/>, <see cref="Forbid"/>, and <see cref="Alias"/> method calls.
    /// The context can be used to evaluate privilege authorization requests.
    /// </returns>
    /// <remarks>
    /// <para>
    /// This method should be called after all rules and aliases have been defined.
    /// The resulting <see cref="PrivilegeContext"/> is immutable and thread-safe,
    /// making it suitable for use across multiple authorization requests.
    /// </para>
    /// <para>
    /// The same <see cref="PrivilegeBuilder"/> instance can be used to build multiple contexts,
    /// but each call to <see cref="Build"/> creates a snapshot of the current state.
    /// </para>
    /// </remarks>
    /// <example>
    /// <code>
    /// var builder = new PrivilegeBuilder()
    ///     .Allow("read", "Post")
    ///     .Forbid("delete", PrivilegeRule.All);
    ///
    /// var context = builder.Build();
    ///
    /// // Use the context for authorization
    /// bool canRead = context.Allowed("read", "Post");        // true
    /// bool canDelete = context.Allowed("delete", "User");    // false
    /// </code>
    /// </example>
    /// <seealso cref="PrivilegeContext"/>
    /// <seealso cref="Allow(string, string, IEnumerable{string}?)"/>
    /// <seealso cref="Forbid(string, string, IEnumerable{string}?)"/>
    /// <seealso cref="Alias(string, IEnumerable{string}, PrivilegeMatch)"/>
    public PrivilegeContext Build()
    {
        return new PrivilegeContext(_rules, _aliases, _stringComparer);
    }

}
