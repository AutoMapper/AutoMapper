namespace AutoMapper
{
    using System.Reflection;

    public interface IMemberGetter : IMemberResolver
    {
        MemberInfo MemberInfo { get; }
        string Name { get; }
        object GetValue(object source);
    }
}