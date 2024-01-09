namespace Privileged;

public static class AuthorizationBuilderExtensions
{
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
