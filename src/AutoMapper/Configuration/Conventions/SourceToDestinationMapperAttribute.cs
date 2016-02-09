namespace AutoMapper.Configuration.Conventions
{
    using System;
    using System.Reflection;

    public abstract class SourceToDestinationMapperAttribute : Attribute
    {
        public abstract bool IsMatch(TypeDetails typeInfo, MemberInfo memberInfo, Type destType, string nameToSearch);
    }
}