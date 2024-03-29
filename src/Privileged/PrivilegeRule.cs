namespace Privileged;

/// <summary>
/// An privilege authorization rule
/// </summary>
/// <param name="Action">The action to match for this rule</param>
/// <param name="Subject">The subject to match for this rule</param>
/// <param name="Fields">The field to match for this rule</param>
/// <param name="Denied">true to make this a denied rule</param>
public record PrivilegeRule(
    string Action,
    string Subject,
    List<string>? Fields = null,
    bool? Denied = null);
