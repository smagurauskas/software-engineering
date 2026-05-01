using System.Text;
using BenchmarkDotNet.Attributes;

[MemoryDiagnoser]
public class MemoryAllocations2
{
    [Benchmark]
    public string StringWithConcatenation()
    {
        string result = "";
        for (int i = 0; i < 1000; i++)
        {
            result = string.Concat(result, i.ToString());
        }
        return result;
    }

    [Benchmark]
    public string StringWithStringBuilder()
    {
        var sb = new StringBuilder();
        for (int i = 0; i < 1000; i++)
        {
            sb.Append(i.ToString());
        }
        return sb.ToString();
    }
}
