using BenchmarkDotNet.Attributes;

public class MemoryLocality3_2
{
    [Benchmark]
    public int[] HeapSpanBased()
    {
        int[] buffer = new int[1_000_000];

        Span<int> span = buffer;
        for (int i = 0; i < span.Length; i++)
            span[i] = i;

        return buffer;
    }

    [Benchmark]
    public int[] HeapArrayBased()
    {
        int[] buffer = new int[1_000_000];

        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = i;

        return buffer;
    }

    [Benchmark]
    public Memory<int> HeapMemoryBased()
    {
        Memory<int> buffer = new int[1_000_000];

        Span<int> span = buffer.Span;
        for (int i = 0; i < span.Length; i++)
            span[i] = i;

        return buffer;
    }
}
