using BenchmarkDotNet.Attributes;

using Bogus;

namespace Privileged.Tests;

[MemoryDiagnoser]
public class AuthorizationContextBenchmarks
{
    private List<AuthorizationRule> _rules;
    private AuthorizationContext _authorziationContext;

    public AuthorizationContextBenchmarks()
    {
        var generator = new Faker<AuthorizationRule>("en_US")
            .UseSeed(2024)
            .CustomInstantiator((f) =>
                new AuthorizationRule
                (
                    Action: f.Random.WeightedRandom(["read", "update", "delete", "all"], [.40f, .30f, .20f, 10f]),
                    Subject: f.Name.FirstName(),
                    Fields: f.Random.WeightedRandom<List<string>?>([null, ["id"]], [.90f, .10f]),
                    Denied: f.Random.WeightedRandom([false, true], [.90f, .10f])
                )
            );

        _rules = generator.Generate(1000);
        _authorziationContext = new AuthorizationContext(_rules);
    }

    [Benchmark]
    public bool AuthorizedReadBenchmark()
    {
        return _authorziationContext.Authorized("read", "Bob");
    }

    [Benchmark]
    public bool AuthorizedAllBenchmark()
    {
        return _authorziationContext.Authorized("delete", "all");
    }

    [Benchmark]
    public bool AuthorizedFieldBenchmark()
    {
        return _authorziationContext.Authorized("update", "Allen", "id");
    }
}
