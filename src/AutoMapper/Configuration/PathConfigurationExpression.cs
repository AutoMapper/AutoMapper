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
        protected List<Action<PathMap>> PathMapActions { get; } = new List<Action<PathMap>>();

        public PathConfigurationExpression(LambdaExpression destinationExpression)
        {
            _destinationExpression = destinationExpression;
            MemberPath = new MemberPath(MemberVisitor.GetMemberPath(destinationExpression));
        }

        public MemberPath MemberPath { get; }

        public MemberInfo DestinationMember => MemberPath.Last;

        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceMember)
        {
            PathMapActions.Add(pm =>
            {
                pm.SourceExpression = sourceMember;
            });
        }

        public void MapFrom(LambdaExpression sourceMember)
        {
            PathMapActions.Add(pm =>
            {
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

            var pathMap = typeMap.FindOrCreatePathMapFor(_destinationExpression, MemberPath, typeMap);

            foreach(var action in PathMapActions)
            {
                action(pathMap);
            }
        }

        public IPropertyMapConfiguration Reverse()
        {
            throw new NotImplementedException();
        }
    }
}