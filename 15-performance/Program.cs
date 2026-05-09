using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

static IConfig MakeConfig(string name) =>
    DefaultConfig.Instance.WithArtifactsPath(
        Path.Combine("BenchmarkReports", $"{DateTime.Now:yyyy-MM-dd_HH-mm-ss}_{name}")
    );

BenchmarkRunner.Run<MemoryAllocations1>(MakeConfig(nameof(MemoryAllocations1)));
BenchmarkRunner.Run<MemoryAllocations2>(MakeConfig(nameof(MemoryAllocations2)));
BenchmarkRunner.Run<MemoryAllocations3>(MakeConfig(nameof(MemoryAllocations3)));

BenchmarkRunner.Run<MemoryLocality1>(MakeConfig(nameof(MemoryLocality1)));
BenchmarkRunner.Run<MemoryLocality2>(MakeConfig(nameof(MemoryLocality2)));
BenchmarkRunner.Run<MemoryLocality3>(MakeConfig(nameof(MemoryLocality3)));
BenchmarkRunner.Run<MemoryLocality3_2>(MakeConfig(nameof(MemoryLocality3_2)));
BenchmarkRunner.Run<MemoryLocality4>(MakeConfig(nameof(MemoryLocality4)));
BenchmarkRunner.Run<MemoryLocality5>(MakeConfig(nameof(MemoryLocality5)));

BenchmarkRunner.Run<BranchPrediction1>(MakeConfig(nameof(BranchPrediction1)));
BenchmarkRunner.Run<BranchPrediction2>(MakeConfig(nameof(BranchPrediction2)));
BenchmarkRunner.Run<BranchPrediction3>(MakeConfig(nameof(BranchPrediction3)));
BenchmarkRunner.Run<BranchPrediction4>(MakeConfig(nameof(BranchPrediction4)));

BenchmarkRunner.Run<Simd1>(MakeConfig(nameof(Simd1)));
BenchmarkRunner.Run<Simd2>(MakeConfig(nameof(Simd2)));
