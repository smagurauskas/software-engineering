using BenchmarkDotNet.Attributes;

public class MemoryAllocations1
{
    [Benchmark]
    public List<int> AllocateIncrementally()
    {
        var list = new List<int>();

        for (int i = 0; i < 1_500_000; i++)
            list.Add(i);

        return list;
    }

    [Benchmark]
    public List<int> AllocateWithCapacity()
    {
        var list = new List<int>(1_500_000);

        for (int i = 0; i < 1_500_000; i++)
            list.Add(i);

        return list;
    }
}
