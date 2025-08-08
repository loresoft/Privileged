using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Diagnosers;
using BenchmarkDotNet.Order;
using BenchmarkDotNet.Running;

namespace Privileged.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var benchmarkSwitcher = new BenchmarkSwitcher(typeof(Program).Assembly);

        var config = ManualConfig.Create(DefaultConfig.Instance)
            .AddDiagnoser(MemoryDiagnoser.Default)
            .WithOption(ConfigOptions.JoinSummary, true)
            .WithOrderer(new DefaultOrderer(SummaryOrderPolicy.FastestToSlowest));

        benchmarkSwitcher.Run(args, config);
    }
}
