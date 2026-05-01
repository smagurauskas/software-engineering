using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

public class Simd1
{
    private readonly int[] RandomNumbers = Enumerable
        .Range(0, 800_000)
        .Select(_ => Random.Shared.Next(0, 10000))
        .ToArray();

    [Benchmark]
    public long BaselineSum()
    {
        long sum = 0;
        foreach (var number in RandomNumbers)
        {
            sum += number;
        }
        return sum;
    }

    [Benchmark]
    public long Avx2Sum_Safe()
    {
        if (!Avx2.IsSupported)
            throw new PlatformNotSupportedException();

        Vector256<int> sumVector = Vector256<int>.Zero;

        for (var i = 0; i <= RandomNumbers.Length - 8; i += 8)
        {
            var vector = Vector256.Create(RandomNumbers, i);
            sumVector = Avx2.Add(sumVector, vector);
        }

        long sum = 0;
        for (int j = 0; j < 8; j++)
        {
            sum += sumVector.GetElement(j);
        }

        return sum;
    }

    [Benchmark]
    public long Avx2Sum_Unsafe()
    {
        if (!Avx2.IsSupported)
            throw new PlatformNotSupportedException();

        Vector256<int> sumVector = Vector256<int>.Zero;
        int i = 0;

        unsafe
        {
            fixed (int* ptr = RandomNumbers)
            {
                for (; i <= RandomNumbers.Length - 8; i += 8)
                {
                    var vector = Avx.LoadVector256(ptr + i);
                    sumVector = Avx2.Add(sumVector, vector);
                }
            }
        }

        long sum = 0;
        for (int j = 0; j < 8; j++)
        {
            sum += sumVector.GetElement(j);
        }

        return sum;
    }

    [Benchmark]
    public long Sse2Sum_Unsafe()
    {
        if (!Sse2.IsSupported)
            throw new PlatformNotSupportedException();

        Vector128<int> sumVector = Vector128<int>.Zero;
        int i = 0;

        unsafe
        {
            fixed (int* ptr = RandomNumbers)
            {
                for (; i <= RandomNumbers.Length - 4; i += 4)
                {
                    var vector = Sse2.LoadVector128(ptr + i);
                    sumVector = Sse2.Add(sumVector, vector);
                }
            }
        }

        long sum = 0;
        for (int j = 0; j < 4; j++)
        {
            sum += sumVector.GetElement(j);
        }

        return sum;
    }

    [Benchmark]
    public long Avx512Sum_Unsafe()
    {
        if (!Avx512F.IsSupported)
            throw new PlatformNotSupportedException();

        Vector512<int> sumVector = Vector512<int>.Zero;
        int i = 0;

        unsafe
        {
            fixed (int* ptr = RandomNumbers)
            {
                for (; i <= RandomNumbers.Length - 16; i += 16)
                {
                    var vector = Avx512F.LoadVector512(ptr + i);
                    sumVector = Avx512F.Add(sumVector, vector);
                }
            }
        }

        long sum = 0;
        for (int j = 0; j < 16; j++)
        {
            sum += sumVector.GetElement(j);
        }

        return sum;
    }
}
