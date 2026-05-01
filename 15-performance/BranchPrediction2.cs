using BenchmarkDotNet.Attributes;

public class BranchPrediction2
{
    public static int[] RandomNumbers = Enumerable
        .Range(0, 1_000_000)
        .OrderBy(_ => Guid.NewGuid())
        .ToArray();

    public static int[] SortedNumbers = RandomNumbers.OrderBy(n => n).ToArray();

    [Benchmark]
    public int SumRandomIfElse()
    {
        int sum = 0;
        foreach (var number in RandomNumbers)
        {
            if (number % 2 == 0)
                sum += number;
            else
                sum -= number;
        }
        return sum;
    }

    [Benchmark]
    public int SumSortedIfElse()
    {
        int sum = 0;
        foreach (var number in SortedNumbers)
        {
            if (number % 2 == 0)
                sum += number;
            else
                sum -= number;
        }
        return sum;
    }
}
