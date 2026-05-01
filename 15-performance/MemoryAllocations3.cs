using System.Buffers;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class MemoryAllocations3
{
    private const int ArraySize = 10_000;
    private const int Iterations = 10_000;

    public long SumArray(int[] array, int length)
    {
        long totalSum = 0;
        for (int i = 0; i < length; i++)
        {
            totalSum += array[i];
        }
        return totalSum;
    }

    [Benchmark(Baseline = true)]
    public long WithAllocatedArrays()
    {
        long acc = 0;

        for (int i = 0; i < Iterations; i++)
        {
            var array = new int[ArraySize];

            for (int j = 0; j < ArraySize; j++)
            {
                array[j] = j;
            }

            acc += SumArray(array, ArraySize);
        }

        return acc;
    }

    [Benchmark]
    public long WithArrayPool()
    {
        long acc = 0;

        for (int i = 0; i < Iterations; i++)
        {
            var array = ArrayPool<int>.Shared.Rent(ArraySize);

            for (int j = 0; j < ArraySize; j++)
            {
                array[j] = j;
            }

            acc += SumArray(array, ArraySize);

            ArrayPool<int>.Shared.Return(array, clearArray: true);
        }

        return acc;
    }
}
