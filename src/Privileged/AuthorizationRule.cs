namespace Authorizone;

public record AuthorizationRule(
    string Action,
    string Subject,
    List<string>? Fields = null,
    bool? Denied = null);
