namespace Privileged;

/// <summary>
/// An <see cref="AuthorizationContext"/> builder extension methods
/// </summary>
public static class AuthorizationBuilderExtensions
{
    /// <summary>
    /// Create a rule allowing the specified <paramref name="actions" />, <paramref name="subjects" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="actions">The actions to allow.</param>
    /// <param name="subjects">The subjects to allow.</param>
    /// <param name="fields">The optional fields to allow.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Allow(this AuthorizationBuilder builder, IEnumerable<string> actions, IEnumerable<string> subjects, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        if (subjects == null)
            throw new ArgumentNullException(nameof(subjects));

        var subjectList = subjects.ToList();

        foreach (var action in actions)
        {
            foreach (var subject in subjectList)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                builder.Allow(action, subject, fields);
            }
        }

        return builder;
    }

    /// <summary>
    /// Create a rule allowing the specified <paramref name="actions" />, <paramref name="subject" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="actions">The actions to allow.</param>
    /// <param name="subject">The subject to allow.</param>
    /// <param name="fields">The optional fields to allow.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Allow(this AuthorizationBuilder builder, IEnumerable<string> actions, string subject, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        foreach (var action in actions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Allow(action, subject, fields);
        }

        return builder;
    }

    /// <summary>
    /// Create a rule allowing the specified <paramref name="action" />, <paramref name="subjects" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="action">The action to allow.</param>
    /// <param name="subjects">The subjects to allow.</param>
    /// <param name="fields">The optional fields to allow.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Allow(this AuthorizationBuilder builder, string action, IEnumerable<string> subjects, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (subjects == null)
            throw new ArgumentNullException(nameof(subjects));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        foreach (var subject in subjects)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Allow(action, subject, fields);
        }

        return builder;
    }

    /// <summary>
    /// Create a rule forbidding the specified <paramref name="actions" />, <paramref name="subjects" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="actions">The actions to forbid.</param>
    /// <param name="subjects">The subjects to forbid.</param>
    /// <param name="fields">The optional fields to forbid.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Forbid(this AuthorizationBuilder builder, IEnumerable<string> actions, IEnumerable<string> subjects, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        if (subjects == null)
            throw new ArgumentNullException(nameof(subjects));

        var subjectList = subjects.ToList();

        foreach (var action in actions)
        {
            foreach (var subject in subjectList)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                builder.Forbid(action, subject, fields);
            }
        }

        return builder;
    }

    /// <summary>
    /// Create a rule forbidding the specified <paramref name="actions" />, <paramref name="subject" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="actions">The actions to forbid.</param>
    /// <param name="subject">The subject to forbid.</param>
    /// <param name="fields">The optional fields to forbid.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Forbid(this AuthorizationBuilder builder, IEnumerable<string> actions, string subject, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (actions == null)
            throw new ArgumentNullException(nameof(actions));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        foreach (var action in actions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Forbid(action, subject, fields);
        }

        return builder;
    }

    /// <summary>
    /// Create a rule forbidding the specified <paramref name="action" />, <paramref name="subjects" /> and optional <paramref name="fields" />.
    /// </summary>
    /// <param name="builder">The <see cref="AuthorizationContext"/> builder.</param>
    /// <param name="action">The action to forbid.</param>
    /// <param name="subjects">The subjects to forbid.</param>
    /// <param name="fields">The optional fields to forbid.</param>
    /// <returns>
    /// The builder for chaining method calls
    /// </returns>
    public static AuthorizationBuilder Forbid(this AuthorizationBuilder builder, string action, IEnumerable<string> subjects, IEnumerable<string>? fields = null)
    {
        if (builder == null)
            throw new ArgumentNullException(nameof(builder));

        if (subjects == null)
            throw new ArgumentNullException(nameof(subjects));

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        foreach (var subject in subjects)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Forbid(action, subject, fields);
        }

        return builder;
    }
}
