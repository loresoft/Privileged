#if NETSTANDARD2_0
namespace System
{
    internal struct HashCode
    {
        private int _hash;

        public void Add<T>(T value)
        {
            int valueHash = value?.GetHashCode() ?? 0;
            _hash = (_hash * 31) + valueHash;
        }

        public readonly int ToHashCode() => _hash;

        public static int Combine<T1, T2>(T1 value1, T2 value2)
        {
            var hash = new HashCode();
            hash.Add(value1);
            hash.Add(value2);
            return hash.ToHashCode();
        }

        public static int Combine<T1, T2, T3>(T1 value1, T2 value2, T3 value3)
        {
            var hash = new HashCode();
            hash.Add(value1);
            hash.Add(value2);
            hash.Add(value3);
            return hash.ToHashCode();
        }

        public static int Combine<T1, T2, T3, T4>(T1 value1, T2 value2, T3 value3, T4 value4)
        {
            var hash = new HashCode();
            hash.Add(value1);
            hash.Add(value2);
            hash.Add(value3);
            hash.Add(value4);
            return hash.ToHashCode();
        }
    }

    namespace Runtime.CompilerServices
    {
        internal sealed class IsExternalInit;

        [AttributeUsage(AttributeTargets.Class | AttributeTargets.Field | AttributeTargets.Property | AttributeTargets.Struct, AllowMultiple = false, Inherited = false)]
        internal sealed class RequiredMemberAttribute : Attribute;

        [AttributeUsage(AttributeTargets.All, AllowMultiple = true, Inherited = false)]
        internal sealed class CompilerFeatureRequiredAttribute : Attribute
        {
            public CompilerFeatureRequiredAttribute(string featureName)
            {
                FeatureName = featureName;
            }

            public string FeatureName { get; }
            public bool IsOptional { get; init; }

            public const string RequiredMembers = nameof(RequiredMembers);
        }
    }
}

#endif
