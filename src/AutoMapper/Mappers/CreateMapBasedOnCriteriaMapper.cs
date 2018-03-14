﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;

namespace AutoMapper.Mappers
{
    public interface IConditionalObjectMapper
    {
        ICollection<Func<TypePair, bool>> Conventions { get; }
        bool IsMatch(in TypePair context);
    }

    public class ConditionalObjectMapper : IConditionalObjectMapper
    {
        public bool IsMatch(in TypePair typePair)
        {
            foreach (var convention in Conventions)
            {
                if (!convention(typePair)) return false;
            }

            return true;
        }

        public ICollection<Func<TypePair, bool>> Conventions { get; } = new Collection<Func<TypePair, bool>>();
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