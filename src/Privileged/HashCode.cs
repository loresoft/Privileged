#if NETSTANDARD2_0
namespace System;

/// <summary>
/// Provides a polyfill for the HashCode class for .NET Standard 2.0.
/// </summary>
internal struct HashCode
{
    private int _hash;

    public void Add<T>(T value)
    {
        int valueHash = value?.GetHashCode() ?? 0;
        _hash = (_hash * 31) + valueHash;
    }

    public readonly int ToHashCode() => _hash;
}
#endif
