using BenchmarkDotNet.Running;

BenchmarkRunner.Run<MemoryAllocations1>();
BenchmarkRunner.Run<MemoryAllocations2>();
BenchmarkRunner.Run<MemoryAllocations3>();

BenchmarkRunner.Run<MemoryLocality1>();
BenchmarkRunner.Run<MemoryLocality2>();
BenchmarkRunner.Run<MemoryLocality3>();
BenchmarkRunner.Run<MemoryLocality3_2>();
BenchmarkRunner.Run<MemoryLocality4>();

BenchmarkRunner.Run<BranchPrediction1>();
BenchmarkRunner.Run<BranchPrediction2>();
BenchmarkRunner.Run<BranchPrediction3>();
BenchmarkRunner.Run<BranchPrediction4>();

BenchmarkRunner.Run<Simd1>();
