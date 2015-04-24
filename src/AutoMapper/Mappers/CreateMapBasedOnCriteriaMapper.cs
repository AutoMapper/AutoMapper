using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using AutoMapper.Impl;

namespace AutoMapper.Mappers
{
    public class CreateMapBasedOnCriteriaMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var contextTypePair = new TypePair(context.SourceType, context.DestinationType);
            Func<TypePair, IObjectMapper> missFunc = tp => (context.Engine as MappingEngine)._mappers.FirstOrDefault(m => m.IsMatch(context));
            var typeMap = mapper.ConfigurationProvider.CreateTypeMap(context.SourceType, context.DestinationType);

            context = context.CreateTypeContext(typeMap, context.SourceValue, context.DestinationValue, context.SourceType, context.DestinationType);

            var map = (context.Engine as MappingEngine)._objectMapperCache.GetOrAdd(contextTypePair, missFunc);
            return map.Map(context, mapper);
        }

        public bool IsMatch(ResolutionContext context)
        {
            return _convensions.All(c => c(context));
        }

        private readonly ICollection<Func<ResolutionContext, bool>> _convensions = new Collection<Func<ResolutionContext, bool>>();

        public static CreateMapBasedOnCriteriaMapper New { get { return new CreateMapBasedOnCriteriaMapper(); } }
        private CreateMapBasedOnCriteriaMapper()
        {
            
        }

        public ICollection<Func<ResolutionContext, bool>> Condition
        {
            get { return _convensions; }
        }

        public bool MatchConvension(ResolutionContext resolutionContext)
        {
            return Condition.All(c => c(resolutionContext));
        }
    }

    public static class ConventionGeneratorExtensions
    {

        public static CreateMapBasedOnCriteriaMapper Postfix(this CreateMapBasedOnCriteriaMapper self, string postFix)
        {
            return self.Where((s, d) => d.Name == s.Name + postFix || s.Name == d.Name + postFix);
        }
        public static CreateMapBasedOnCriteriaMapper Where(this CreateMapBasedOnCriteriaMapper self, Func<Type, Type, bool> condition)
        {
            self.Condition.Add(rc => condition(rc.SourceType, rc.DestinationType));
            return self;
        }

    }

}