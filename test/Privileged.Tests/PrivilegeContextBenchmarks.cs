using BenchmarkDotNet.Attributes;

using Bogus;

namespace Privileged.Tests;

[MemoryDiagnoser]
public class PrivilegeContextBenchmarks
{
    private List<PrivilegeRule> _rules;
    private PrivilegeContext _authorziationContext;

    public PrivilegeContextBenchmarks()
    {
        var generator = new Faker<PrivilegeRule>("en_US")
            .UseSeed(2024)
            .CustomInstantiator((f) =>
                new PrivilegeRule
                (
                    Action: f.Random.WeightedRandom(["read", "update", "delete", "all"], [.40f, .30f, .20f, 10f]),
                    Subject: f.Name.FirstName(),
                    Fields: f.Random.WeightedRandom<List<string>?>([null, ["id"]], [.90f, .10f]),
                    Denied: f.Random.WeightedRandom([false, true], [.90f, .10f])
                )
            );

        _rules = generator.Generate(1000);
        _authorziationContext = new PrivilegeContext(_rules);
    }

    [Benchmark]
    public bool AuthorizedReadBenchmark()
    {
        return _authorziationContext.Allowed("read", "Bob");
    }

    [Benchmark]
    public bool AuthorizedAllBenchmark()
    {
        return _authorziationContext.Allowed("delete", "all");
    }

    [Benchmark]
    public bool AuthorizedFieldBenchmark()
    {
        return _authorziationContext.Allowed("update", "Allen", "id");
    }
}
