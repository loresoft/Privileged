namespace Privileged;

public class AuthorizationBuilder
{
    private readonly List<AuthorizationRule> _rules = [];

    public AuthorizationBuilder Allow(string action, string subject, IEnumerable<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        var rule = new AuthorizationRule(action, subject, fields?.ToList());
        _rules.Add(rule);

        return this;
    }

    public AuthorizationBuilder Forbid(string action, string subject, IEnumerable<string>? fields = null)
    {
        if (string.IsNullOrWhiteSpace(action))
            throw new ArgumentException("Action cannot be null or whitespace.", nameof(action));

        if (string.IsNullOrWhiteSpace(subject))
            throw new ArgumentException("Subject cannot be null or whitespace.", nameof(subject));

        var rule = new AuthorizationRule(action, subject, fields?.ToList(), true);
        _rules.Add(rule);

        return this;
    }

    public AuthorizationContext Build()
    {
        return new AuthorizationContext(_rules);
    }
}
