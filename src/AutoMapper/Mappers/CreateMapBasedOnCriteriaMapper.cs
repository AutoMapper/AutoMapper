using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper.Impl;

namespace AutoMapper.Mappers
{
    public interface IConditionalObjectMapper : IObjectMapper
    {
        ICollection<Func<ResolutionContext, bool>> Conventions { get; }
    }

    public class ConditionalObjectMapper : IConditionalObjectMapper
    {
        private readonly string _profileName;

        public ConditionalObjectMapper(string profileName)
        {
            _profileName = profileName;
        }

        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var contextTypePair = new TypePair(context.SourceType, context.DestinationType);
            Func<TypePair, IObjectMapper> missFunc = tp => context.Engine.ConfigurationProvider.GetMappers().FirstOrDefault(m => m.IsMatch(context));
            var typeMap = mapper.ConfigurationProvider.CreateTypeMap(context.SourceType, context.DestinationType, _profileName);

            context = context.CreateTypeContext(typeMap, context.SourceValue, context.DestinationValue, context.SourceType, context.DestinationType);

            var map = (context.Engine as MappingEngine)._objectMapperCache.GetOrAdd(contextTypePair, missFunc);
            return map.Map(context, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
            return Conventions.All(c => c(context));
        }

        public ICollection<Func<ResolutionContext, bool>> Conventions { get; } = new Collection<Func<ResolutionContext, bool>>();
    }

    public static class ConventionGeneratorExtensions
    {
        public static IConditionalObjectMapper Where(this IConditionalObjectMapper self, Func<Type, Type, bool> condition)
        {
            self.Conventions.Add(rc => condition(rc.SourceType, rc.DestinationType));
            return self;
        }

    }

}