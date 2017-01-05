namespace AutoMapper
{
    using System;
    using System.Linq.Expressions;

    public class ValueResolverConfiguration
    {
        public object Instance { get; }
        public Type Type { get; }
        public Type InterfaceType { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }

        public ValueResolverConfiguration(Type type, Type interfaceType)
        {
            Type = type;
            InterfaceType = interfaceType;
        }

        public ValueResolverConfiguration(object instance, Type interfaceType)
        {
            Instance = instance;
            InterfaceType = interfaceType;
        }
    }
}