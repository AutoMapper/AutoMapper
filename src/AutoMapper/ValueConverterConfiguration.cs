using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public class ValueConverterConfiguration
    {
        public object Instance { get; }
        public Type ConcreteType { get; }
        public Type InterfaceType { get; }
        public LambdaExpression SourceMember { get; set; }
        public string SourceMemberName { get; set; }

        public ValueConverterConfiguration(Type concreteType, Type interfaceType)
        {
            ConcreteType = concreteType;
            InterfaceType = interfaceType;
        }

        public ValueConverterConfiguration(object instance, Type interfaceType)
        {
            Instance = instance;
            ConcreteType = instance.GetType();
            InterfaceType = interfaceType;
        }
    }
}