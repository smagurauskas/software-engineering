using BenchmarkDotNet.Attributes;

public class MemoryLocality2
{
    public struct NumberContainerStruct
    {
        public int Number;
    }

    public class NumberContainerClass
    {
        public int Number;
    }

    [Benchmark]
    public void StructArray()
    {
        NumberContainerStruct[] buffer = new NumberContainerStruct[100_000];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i].Number = i;

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i].Number;
    }

    [Benchmark]
    public void ClassArray()
    {
        NumberContainerClass[] buffer = new NumberContainerClass[100_000];
        for (int i = 0; i < buffer.Length; i++)
            buffer[i] = new NumberContainerClass { Number = i };

        int sum = 0;
        for (int i = 0; i < buffer.Length; i++)
            sum += buffer[i].Number;
    }
}
