using BenchmarkDotNet.Attributes;

public class MemoryLocality3
{
    private readonly int[] IntegersOnHeap = Enumerable.Range(0, 100_000).ToArray();

    [Benchmark]
    public void HeapSpanBased()
    {
        Span<int> buffer = IntegersOnHeap;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i];
    }

    [Benchmark]
    public void HeapArrayBased()
    {
        int[] buffer = IntegersOnHeap;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i];
    }

    [Benchmark]
    public void HeapMemoryBased()
    {
        Memory<int> buffer = IntegersOnHeap;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer.Span[i];
    }
}
