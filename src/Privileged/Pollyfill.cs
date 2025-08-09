#if NETSTANDARD2_0
namespace System
{
    namespace Diagnostics.CodeAnalysis
    {
        [AttributeUsage(AttributeTargets.Constructor, AllowMultiple = false, Inherited = false)]
        internal sealed class SetsRequiredMembersAttribute : Attribute;
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
