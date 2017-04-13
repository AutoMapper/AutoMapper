using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    public class PathConfigurationExpression<TSource, TDestination, TMember> : IPathConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
    {
        private readonly LambdaExpression _destinationExpression;
        private readonly Type _sourceType;
        protected List<Action<PathMap>> PathMapActions { get; } = new List<Action<PathMap>>();

        public PathConfigurationExpression(Expression<Func<TDestination, TMember>> destinationExpression, Type sourceType)
        {
            _destinationExpression = destinationExpression;
            MemberPath = new MemberPath(MemberVisitor.GetMemberPath(destinationExpression));
            _sourceType = sourceType;
        }

        public MemberPath MemberPath { get; }

        public MemberInfo DestinationMember => ((MemberExpression)_destinationExpression.Body).Member;

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            PathMapActions.Add(pm =>
            {
                pm.DestinationExpression = _destinationExpression;
                pm.SourceExpression = sourceMember;
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

            var pathMap = typeMap.FindOrCreatePathMapFor(MemberPath, typeMap);

            foreach(var action in PathMapActions)
            {
                action(pathMap);
            }
        }
    }
}