using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

[SimpleJob(RuntimeMoniker.Net90)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[SimpleJob(RuntimeMoniker.NativeAot90)]
public class MemoryLocality4
{
    [Benchmark]
    public void InterleavedArrayTuple()
    {
        var buffer = new (int A, int B)[1_000_000];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i].A = i;
            buffer[i].B = i * 2;
        }

        int sumA = 0;
        int sumB = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            sumA += buffer[i].A;
            sumB += buffer[i].B;
        }
    }

    public struct NumberPairStruct
    {
        public int A;
        public int B;
    }

    [Benchmark]
    public void InterleavedArrayStruct()
    {
        var buffer = new NumberPairStruct[1_000_000];
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i].A = i;
            buffer[i].B = i * 2;
        }

        int sumA = 0;
        int sumB = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            sumA += buffer[i].A;
            sumB += buffer[i].B;
        }
    }

    [Benchmark]
    public void SplitArray()
    {
        var bufferA = new int[1_000_000];
        var bufferB = new int[1_000_000];
        for (int i = 0; i < bufferA.Length; i++)
        {
            bufferA[i] = i;
            bufferB[i] = i * 2;
        }

        int sumA = 0;
        int sumB = 0;
        for (int i = 0; i < bufferA.Length; i++)
        {
            sumA += bufferA[i];
            sumB += bufferB[i];
        }
    }
}
