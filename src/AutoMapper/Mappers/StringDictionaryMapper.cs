using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.Mappers
{
    using System;

    public class ToStringDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.DestinationType);
        }

        public object Map(ResolutionContext context)
        {
            var source = context.SourceValue;
            var sourceType = source.GetType();
            var sourceTypeDetails = new TypeDetails(sourceType, _ => true, _ => true);
            var membersDictionary = sourceTypeDetails.PublicReadAccessors.ToDictionary(p => p.Name, p => p.GetMemberValue(source));
            var newContext = context.CreateTypeContext(null, membersDictionary, context.DestinationValue, membersDictionary.GetType(), context.DestinationType);
            return context.Engine.Map(newContext);
        }
    }

    public class FromStringDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(TypePair context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.SourceType);
        }

        public object Map(ResolutionContext context)
        {
            var dictionary = (StringDictionary)context.SourceValue;
            object destination = context.Engine.CreateObject(context);
            var destTypeDetails = new TypeDetails(context.DestinationType, _ => true, _ => true);
            var members = from name in dictionary.Keys join member in destTypeDetails.PublicWriteAccessors on name equals member.Name select member;
            foreach(var member in members)
            {
                object value = ReflectionHelper.Map(context, member, dictionary[member.Name]);
                member.SetMemberValue(destination, value);
            }
            return destination;
        }
    }
}