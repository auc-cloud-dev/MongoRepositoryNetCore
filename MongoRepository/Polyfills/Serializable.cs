namespace System
{
#if NETCOREAPP1_0 
    [AttributeUsage(AttributeTargets.Class | AttributeTargets.Struct | AttributeTargets.Enum | AttributeTargets.Delegate)]
    public class SerializableAttribute : Attribute
    {
    }
#endif
}
