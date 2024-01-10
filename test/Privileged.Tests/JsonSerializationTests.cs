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
        var context = new AuthorizationBuilder()
            .Allow("test", AuthorizationSubjects.All)
            .Allow(AuthorizationActions.All, "Post")
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

        var rules = JsonSerializer.Deserialize<List<AuthorizationRule>>(json, options);
        rules.Should().NotBeNullOrEmpty();
        rules.Count.Should().Be(4);

        var jsonContext = new AuthorizationContext(rules);
        jsonContext.Should().NotBeNull();

        var contextJson = JsonSerializer.Serialize(jsonContext, options);
        contextJson.Should().NotBeNullOrEmpty();
    }
}

[JsonSerializable(typeof(AuthorizationContext))]
[JsonSerializable(typeof(AuthorizationRule))]
[JsonSerializable(typeof(List<AuthorizationRule>))]
[JsonSerializable(typeof(IReadOnlyCollection<AuthorizationRule>))]
public partial class AuthorizationJsonContext : JsonSerializerContext;
