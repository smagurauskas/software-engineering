using System.Numerics;
using System.Runtime.InteropServices;
using System.Runtime.Intrinsics;
using System.Runtime.Intrinsics.X86;
using BenchmarkDotNet.Attributes;

public class Simd2
{
    private readonly byte[] LoremIpsum = MemoryMarshal
        .AsBytes(File.ReadAllText("LoremIpsum.txt").AsSpan())
        .ToArray();

    [Benchmark]
    public long LoopedSearch()
    {
        string needle = "cat";
        long index = -1;

        for (int i = 0; i <= LoremIpsum.Length - needle.Length; i++)
        {
            if (
                LoremIpsum[i] == needle[0]
                && LoremIpsum[i + 1] == needle[1]
                && LoremIpsum[i + 2] == needle[2]
            )
            {
                index = i;
                break;
            }
        }

        return index;
    }

    [Benchmark]
    public long SimdSearch()
    {
        string needle = "cat";
        long index = -1;

        if (!Avx2.IsSupported)
            throw new PlatformNotSupportedException();

        unsafe
        {
            fixed (byte* ptr = LoremIpsum)
            {
                var maskLeft = Vector256.Create((ushort)needle[0]);
                var maskRight = Vector256.Create((ushort)needle[needle.Length - 1]);

                for (int i = 0; i <= LoremIpsum.Length - needle.Length - 16; i += 16)
                {
                    var left = Avx2.CompareEqual(maskLeft, Avx.LoadVector256((ushort*)(ptr + i)));
                    var right = Avx2.CompareEqual(
                        maskRight,
                        Avx.LoadVector256((ushort*)(ptr + i + needle.Length - 1))
                    );
                    var combined = Avx2.And(left, right);

                    int bitMask = Avx2.MoveMask(combined.AsByte());

                    while (bitMask != 0)
                    {
                        int byteOffset = BitOperations.TrailingZeroCount(bitMask);
                        int charOffset = byteOffset / 2;
                        int candidate = i + charOffset;

                        if (
                            candidate <= LoremIpsum.Length - needle.Length
                            && LoremIpsum[candidate] == needle[0]
                            && LoremIpsum[candidate + 1] == needle[1]
                            && LoremIpsum[candidate + 2] == needle[2]
                        )
                        {
                            index = candidate;
                            break;
                        }
                        bitMask &= bitMask - 1;
                    }
                    if (index != -1)
                        break;
                }
            }
            return index;
        }
    }
}
