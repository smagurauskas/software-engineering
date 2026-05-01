using BenchmarkDotNet.Attributes;

public class BranchPrediction3
{
    public static int[] RandomNumbers = Enumerable
        .Range(0, 1_000_000)
        .OrderBy(_ => Guid.NewGuid())
        .ToArray();

    public static int[] SortedNumbers = RandomNumbers.OrderBy(n => n).ToArray();

    [Benchmark]
    public int CountEvenRandom()
    {
        int count = 0;

        foreach (var number in RandomNumbers)
        {
            if (number % 2 == 0)
                count++;
        }

        return count;
    }

    [Benchmark]
    public int CountEvenSorted()
    {
        int count = 0;

        foreach (var number in SortedNumbers)
        {
            if (number % 2 == 0)
                count++;
        }

        return count;
    }
}
