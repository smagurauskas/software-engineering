using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class MemoryLocality5
{
    [Benchmark]
    public long SumShorts()
    {
        var shortData = new short[1_000_000];
        for (var i = 0; i < 1_000_000; i++)
        {
            shortData[i] = (short)i;
        }

        var acc = 0L;
        for (int i = 0; i < shortData.Length; i++)
        {
            acc += shortData[i];
        }
        return acc;
    }

    [Benchmark]
    public long SumInts()
    {
        var intData = new int[1_000_000];
        for (var i = 0; i < 1_000_000; i++)
        {
            intData[i] = i;
        }

        var acc = 0L;
        for (int i = 0; i < intData.Length; i++)
        {
            acc += intData[i];
        }
        return acc;
    }

    [Benchmark]
    public long SumLongs()
    {
        var longData = new long[1_000_000];
        for (var i = 0; i < 1_000_000; i++)
        {
            longData[i] = i;
        }

        var acc = 0L;
        for (int i = 0; i < longData.Length; i++)
        {
            acc += longData[i];
        }
        return acc;
    }
}
