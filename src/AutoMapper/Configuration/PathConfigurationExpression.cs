using System;
using System.Linq;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class PathConfigurationExpression<TSource, TDestination> : IPathConfigurationExpression<TSource, TDestination>, IPropertyMapConfiguration
    {
        private readonly LambdaExpression _destinationExpression;
        private LambdaExpression _sourceExpression;
        protected List<Action<PathMap>> PathMapActions { get; } = new List<Action<PathMap>>();

        public PathConfigurationExpression(LambdaExpression destinationExpression)
        {
            _destinationExpression = destinationExpression;
            MemberPath = new MemberPath(MemberVisitor.GetMemberPath(destinationExpression).Reverse());
        }

        public MemberPath MemberPath { get; }

        public MemberInfo DestinationMember => MemberPath.Last;

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression)
        {
            MapFromUntyped(sourceExpression);
        }

        public void MapFromUntyped(LambdaExpression sourceExpression)
        {
            _sourceExpression = sourceExpression;
            PathMapActions.Add(pm =>
            {
                pm.SourceExpression = sourceExpression;
            });
        }

        public void Configure(TypeMap typeMap)
        {
            //var destMember = DestinationMember;

            //if(destMember.DeclaringType.IsGenericType())
            //{
            //    var destTypeInfo = typeMap.Profile.CreateTypeDetails(destMember.DeclaringType);
            //    destMember = destTypeInfo.PublicReadAccessors.Single(m => m.Name == destMember.Name);
            //}

            var pathMap = typeMap.FindOrCreatePathMapFor(_destinationExpression, MemberPath, typeMap);

            Apply(pathMap);
        }

        private void Apply(PathMap pathMap)
        {
            foreach(var action in PathMapActions)
            {
                action(pathMap);
            }
        }

        internal static IPropertyMapConfiguration Create(LambdaExpression destination, LambdaExpression source)
        {
            if(destination == null || !destination.IsMemberPath())
            {
                return null;
            }
            var reversed = new PathConfigurationExpression<TSource, TDestination>(destination);
            if(reversed.MemberPath.Length == 1)
            {
                var reversedMemberExpression = new MemberConfigurationExpression<TSource, TDestination, object>(reversed.DestinationMember, typeof(TSource));
                reversedMemberExpression.MapFromUntyped(source);
                return reversedMemberExpression;
            }
            reversed.MapFromUntyped(source);
            return reversed;
        }

        public IPropertyMapConfiguration Reverse()
        {
            return Create(_sourceExpression, _destinationExpression);
        }
    }
}