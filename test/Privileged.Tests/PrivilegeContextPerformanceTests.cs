using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Exporters;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Loggers;
using BenchmarkDotNet.Running;

using Xunit.Abstractions;

namespace Privileged.Tests;

public class PrivilegeContextPerformanceTests
{
    private readonly ITestOutputHelper _output;

    public PrivilegeContextPerformanceTests(ITestOutputHelper output)
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

        var summary = BenchmarkRunner.Run<PrivilegeContextBenchmarks>(config);

        // write benchmark summary
        var logger = new AccumulationLogger();

        MarkdownExporter.Console.ExportToLog(summary, logger);

        _output.WriteLine(logger.GetLog());
    }
}
