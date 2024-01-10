namespace Privileged;

/// <summary>
/// An <see cref="PrivilegeContext"/> builder
/// </summary>
public class PrivilegeBuilder
{
    private readonly List<PrivilegeRule> _rules = [];

    /// <summary>
    /// Create a rule allowing the specified <paramref name="action"/>, <paramref name="subject"/> and optional <paramref name="fields"/>.
    /// </summary>
    /// <param name="action">The action to allow.</param>
    /// <param name="subject">The subject to allow.</param>
    /// <param name="fields">The optional fields to allow.</param>
    /// <returns>The builder for chaining method calls</returns>
    /// <exception cref="ArgumentException">
    /// Action or Subject cannot be null or whitespace.
    /// </exception>
    public PrivilegeBuilder Allow(string action, string subject, IEnumerable<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        var rule = new PrivilegeRule(action, subject, fields?.ToList());
        _rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Create a rule forbidding the specified <paramref name="action"/>, <paramref name="subject"/> and optional <paramref name="fields"/>.
    /// </summary>
    /// <param name="action">The action to forbid.</param>
    /// <param name="subject">The subject to forbid.</param>
    /// <param name="fields">The optional fields to forbid.</param>
    /// <returns>The builder for chaining method calls</returns>
    /// <exception cref="ArgumentException">
    /// Action or Subject cannot be null or whitespace.
    /// </exception>
    public PrivilegeBuilder Forbid(string action, string subject, IEnumerable<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        var rule = new PrivilegeRule(action, subject, fields?.ToList(), true);
        _rules.Add(rule);

        return this;
    }

    /// <summary>
    /// Creates the <see cref="PrivilegeContext"/> from the rules specified in <see cref="Allow"/> or <see cref="Forbid"/> methods.
    /// </summary>
    /// <returns>An instance of <see cref="PrivilegeContext"/> with the specified rules</returns>
    /// <seealso cref="Allow"/>
    /// <seealso cref="Forbid"/>
    public PrivilegeContext Build()
    {
        return new PrivilegeContext(_rules);
    }
}
