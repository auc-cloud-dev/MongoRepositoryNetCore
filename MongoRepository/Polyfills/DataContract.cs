namespace System.Runtime.Serialization
{
#if NETCOREAPP1_0 
    [AttributeUsageAttribute(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum, Inherited = false, AllowMultiple = false)]
    public class DataContractAttribute : Attribute
    {
    }
#endif
}
