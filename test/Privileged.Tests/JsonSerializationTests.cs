using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

using Xunit.Abstractions;

namespace Privileged.Tests;

public class JsonSerializationTests
{
    private readonly ITestOutputHelper _output;

    public JsonSerializationTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void SerializationRules()
    {
        var context = new PrivilegeBuilder()
            .Allow("test", PrivilegeSubjects.All)
            .Allow(PrivilegeActions.All, "Post")
            .Allow("read", "User", ["title", "id"])
            .Forbid("publish", "Post")
            .Build();

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = AuthorizationJsonContext.Default
        };

        var json = JsonSerializer.Serialize(context.Rules, options);
        json.Should().NotBeNullOrEmpty();

        _output.WriteLine(json);

        var rules = JsonSerializer.Deserialize<List<PrivilegeRule>>(json, options);
        rules.Should().NotBeNullOrEmpty();
        rules.Count.Should().Be(4);
    }

    [Fact]
    public void SerializationPrivilegeModel()
    {
        var rules = new List<PrivilegeRule>
        {
            new() { Action = "read", Subject = "Post" },
            new() { Action = "write", Subject = "User", Qualifiers = ["title", "content"] }
        };

        var aliases = new List<PrivilegeAlias>
        {
            new() { Alias = "modify", Values = ["create", "update", "delete"], Type = PrivilegeMatch.Action },
            new() { Alias = "entity", Values = ["Post", "User"], Type = PrivilegeMatch.Subject }
        };

        var model = new PrivilegeModel
        {
            Rules = rules,
            Aliases = aliases
        };

        var options = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            TypeInfoResolver = AuthorizationJsonContext.Default
        };

        var json = JsonSerializer.Serialize(model, options);
        json.Should().NotBeNullOrEmpty();

        _output.WriteLine(json);

        var deserializedContext = JsonSerializer.Deserialize<PrivilegeModel>(json, options);

        deserializedContext.Should().NotBeNull();
        deserializedContext.Rules.Should().HaveCount(2);
        deserializedContext.Aliases.Should().HaveCount(2);
    }
}

[JsonSerializable(typeof(PrivilegeModel))]
[JsonSerializable(typeof(PrivilegeRule))]
[JsonSerializable(typeof(List<PrivilegeRule>))]
[JsonSerializable(typeof(IReadOnlyCollection<PrivilegeRule>))]
public partial class AuthorizationJsonContext : JsonSerializerContext;
