using System.Linq;
using System.Reflection;
using AutoMapper.Internal;
using StringDictionary = System.Collections.Generic.IDictionary<string, object>;

namespace AutoMapper.Mappers
{
    public class ToStringDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(ResolutionContext context)
        {
            return typeof(StringDictionary).IsAssignableFrom(context.DestinationType);
        }

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var source = context.SourceValue;
            var membersDictionary = source.GetType().GetReadableAccesors().ToDictionary(p => p.Name, p => p.GetMemberValue(source));
            var newContext = context.CreateTypeContext(null, membersDictionary, context.DestinationValue, membersDictionary.GetType(), context.DestinationType);
            return mapper.Map(newContext);
        }
    }

    public class FromStringDictionaryMapper : IObjectMapper
    {
        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceValue is StringDictionary;
        }

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var dictionary = (StringDictionary)context.SourceValue;
            object destination = mapper.CreateObject(context);
            var members = from name in dictionary.Keys join member in context.DestinationType.GetWritableAccesors() on name equals member.Name select member;
            foreach(var member in members)
            {
                object value = ReflectionHelper.Map(member, dictionary[member.Name]);
                member.SetMemberValue(destination, value);
            }
            return destination;
        }
    }
}