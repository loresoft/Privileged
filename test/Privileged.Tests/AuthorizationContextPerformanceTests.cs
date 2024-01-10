using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using Xunit.Abstractions;

namespace Privileged.Tests;

public class AuthorizationContextPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public AuthorizationContextPerformanceTests(ITestOutputHelper output)
    {
        _output = output;
    }

    [Fact]
    public void BenchmarkRunnerTest()
    {
        var config = ManualConfig
            .CreateMinimumViable()
            .AddJob(Job.ShortRun)
            .WithOptions(ConfigOptions.DisableOptimizationsValidator)
            .WithOption(ConfigOptions.JoinSummary, true);

        var summary = BenchmarkRunner.Run<AuthorizationContextBenchmarks>(config);

        // write benchmark summary
        var logger = new AccumulationLogger();

        MarkdownExporter.Console.ExportToLog(summary, logger);

        _output.WriteLine(logger.GetLog());
    }
}
