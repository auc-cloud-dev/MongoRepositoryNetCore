namespace System.Runtime.Serialization
{
#if NETCOREAPP1_0 
    [AttributeUsageAttribute(AttributeTargets.Property | AttributeTargets.Field, Inherited = false, AllowMultiple = false)]
    public class DataMemberAttribute : Attribute
    {
    }
#endif
}
