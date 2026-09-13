namespace GraphQlApi.Storage;

public sealed class ReadCounter
{
    public const string ItemKey = "catalog-file-reads";

    private int value;

    public int Value => value;

    public int Increment() => Interlocked.Increment(ref value);
}
