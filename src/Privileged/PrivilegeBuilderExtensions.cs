namespace Privileged;

/// <summary>
/// Extension methods for the <see cref="PrivilegeBuilder"/> to simplify bulk rule definitions.
/// </summary>
/// <remarks>
/// These methods provide convenience for defining multiple privilege rules using collections of actions and/or subjects.
/// </remarks>
public static class PrivilegeBuilderExtensions
{
    /// <summary>
    /// Adds rules that allow each combination of the specified <paramref name="actions"/> and <paramref name="subjects"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="actions">A collection of actions to allow.</param>
    /// <param name="subjects">A collection of subjects to allow.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">
    /// Thrown if <paramref name="builder"/>, <paramref name="actions"/>, or <paramref name="subjects"/> is null.
    /// </exception>
    public static PrivilegeBuilder Allow(this PrivilegeBuilder builder, IEnumerable<string> actions, IEnumerable<string> subjects, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(subjects);

        var subjectList = subjects.ToList();

        foreach (var action in actions)
        {
            foreach (var subject in subjectList)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                builder.Allow(action, subject, qualifiers);
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds rules that allow the specified <paramref name="actions"/> for a single <paramref name="subject"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="actions">A collection of actions to allow.</param>
    /// <param name="subject">The subject to allow the actions for.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="actions"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="subject"/> is null or whitespace.</exception>
    public static PrivilegeBuilder Allow(this PrivilegeBuilder builder, IEnumerable<string> actions, string subject, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(actions);

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        foreach (var action in actions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Allow(action, subject, qualifiers);
        }

        return builder;
    }

    /// <summary>
    /// Adds rules that allow a single <paramref name="action"/> for each of the specified <paramref name="subjects"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="action">The action to allow.</param>
    /// <param name="subjects">A collection of subjects to allow the action for.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="subjects"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="action"/> is null or whitespace.</exception>
    public static PrivilegeBuilder Allow(this PrivilegeBuilder builder, string action, IEnumerable<string> subjects, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(subjects);

        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        foreach (var subject in subjects)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Allow(action, subject, qualifiers);
        }

        return builder;
    }

    /// <summary>
    /// Adds rules that forbid each combination of the specified <paramref name="actions"/> and <paramref name="subjects"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="actions">A collection of actions to forbid.</param>
    /// <param name="subjects">A collection of subjects to forbid the actions for.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/>, <paramref name="actions"/>, or <paramref name="subjects"/> is null.</exception>
    public static PrivilegeBuilder Forbid(this PrivilegeBuilder builder, IEnumerable<string> actions, IEnumerable<string> subjects, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentNullException.ThrowIfNull(subjects);

        var subjectList = subjects.ToList();

        foreach (var action in actions)
        {
            foreach (var subject in subjectList)
            {
                // ReSharper disable once PossibleMultipleEnumeration
                builder.Forbid(action, subject, qualifiers);
            }
        }

        return builder;
    }

    /// <summary>
    /// Adds rules that forbid the specified <paramref name="actions"/> for a single <paramref name="subject"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="actions">A collection of actions to forbid.</param>
    /// <param name="subject">The subject to forbid the actions for.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="actions"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="subject"/> is null or whitespace.</exception>
    public static PrivilegeBuilder Forbid(this PrivilegeBuilder builder, IEnumerable<string> actions, string subject, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(actions);
        ArgumentException.ThrowIfNullOrWhiteSpace(subject);

        foreach (var action in actions)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Forbid(action, subject, qualifiers);
        }

        return builder;
    }

    /// <summary>
    /// Adds rules that forbid a single <paramref name="action"/> for each of the specified <paramref name="subjects"/>, optionally scoped by <paramref name="qualifiers"/>.
    /// </summary>
    /// <param name="builder">The privilege builder instance.</param>
    /// <param name="action">The action to forbid.</param>
    /// <param name="subjects">A collection of subjects to forbid the action for.</param>
    /// <param name="qualifiers">Optional qualifiers to apply to each rule.</param>
    /// <returns>The same <see cref="PrivilegeBuilder"/> instance to support method chaining.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="builder"/> or <paramref name="subjects"/> is null.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="action"/> is null or whitespace.</exception>
    public static PrivilegeBuilder Forbid(this PrivilegeBuilder builder, string action, IEnumerable<string> subjects, IEnumerable<string>? qualifiers = null)
    {
        ArgumentNullException.ThrowIfNull(builder);
        ArgumentNullException.ThrowIfNull(subjects);
        ArgumentException.ThrowIfNullOrWhiteSpace(action);

        foreach (var subject in subjects)
        {
            // ReSharper disable once PossibleMultipleEnumeration
            builder.Forbid(action, subject, qualifiers);
        }

        return builder;
    }
}
