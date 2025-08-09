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
            new PrivilegeRule { Action = "read", Subject = "Post" },
            new PrivilegeRule { Action = "write", Subject = "Post", Qualifiers = ["title", "content"] },
            new PrivilegeRule { Action = "delete", Subject = "Post", Denied = true },
            new PrivilegeRule { Action = "read", Subject = "User", Qualifiers = ["id", "name"] },
            new PrivilegeRule { Action = "update", Subject = "User", Denied = true }
        };

        var aliases = new[]
        {
            new PrivilegeAlias { Alias = "modify", Values = ["create", "update", "delete"], Type = PrivilegeMatch.Action },
            new PrivilegeAlias { Alias = "content", Values = ["title", "content"], Type = PrivilegeMatch.Qualifier },
            new PrivilegeAlias { Alias = "entity", Values = ["Post", "User"], Type = PrivilegeMatch.Subject }
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
    public int BenchmarkMatchRulesWithAlias()
    {
        var result = _contextWithAliases.MatchRules("modify", "entity", "content");
        return result.Count;
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

    [Benchmark]
    public int BenchmarkMatchRulesWithoutAlias()
    {
        var result = _contextWithoutAliases.MatchRules("modify", "entity", "content");
        return result.Count;
    }

}
