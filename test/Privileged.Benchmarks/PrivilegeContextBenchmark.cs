using System;
using System.Collections.Generic;
using BenchmarkDotNet.Attributes;

namespace Privileged.Benchmarks;

[MemoryDiagnoser]
public class PrivilegeContextBenchmark
{
    private PrivilegeContext _contextWithAliases = null!;
    private PrivilegeContext _contextWithoutAliases = null!;

    [GlobalSetup]
    public void Setup()
    {
        var rules = new[]
        {
            new PrivilegeRule("read", "Post"),
            new PrivilegeRule("write", "Post", ["title", "content"]),
            new PrivilegeRule("delete", "Post", Denied: true),
            new PrivilegeRule("read", "User", ["id", "name"]),
            new PrivilegeRule("update", "User", Denied: true)
        };

        var aliases = new[]
        {
            new PrivilegeAlias("modify", ["create", "update", "delete"], PrivilegeMatch.Action),
            new PrivilegeAlias("content", ["title", "content"], PrivilegeMatch.Qualifier),
            new PrivilegeAlias("entity", ["Post", "User"], PrivilegeMatch.Subject)
        };

        _contextWithAliases = new PrivilegeContext(rules, aliases);
        _contextWithoutAliases = new PrivilegeContext(rules);
    }

    [Benchmark]
    public bool BenchmarkAllowedWithAlias()
    {
        return _contextWithAliases.Allowed("modify", "entity", "content");
    }

    [Benchmark]
    public bool BenchmarkForbiddenWithAlias()
    {
        return _contextWithAliases.Forbidden("modify", "entity", "content");
    }


    [Benchmark]
    public bool BenchmarkAllowedWithoutAlias()
    {
        return _contextWithoutAliases.Allowed("update", "Post", "title");
    }

    [Benchmark]
    public bool BenchmarkForbiddenWithoutAlias()
    {
        return _contextWithoutAliases.Forbidden("delete", "Post", "content");
    }

}
