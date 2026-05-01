using BenchmarkDotNet.Attributes;

public class MemoryLocality1
{
    [Benchmark]
    public void StackBased()
    {
        Span<int> buffer = stackalloc int[100_000];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = i;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i];
    }

    [Benchmark]
    public void HeapBasedWithBoxing()
    {
        int[] buffer = new int[100_000];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = i;

        object sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum = (int)sum + buffer[i];
    }

    [Benchmark]
    public void HeapBasedNoBoxing()
    {
        int[] buffer = new int[100_000];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = i;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i];
    }
}
