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
