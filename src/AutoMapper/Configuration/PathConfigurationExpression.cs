using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper.Configuration
{
    using Execution;
    public class PathConfigurationExpression<TSource, TDestination, TMember> : IPathConfigurationExpression<TSource, TDestination, TMember>, IPropertyMapConfiguration
    {
        private readonly LambdaExpression _destinationExpression;
        private LambdaExpression _sourceExpression;
        protected List<Action<PathMap>> PathMapActions { get; } = new List<Action<PathMap>>();

        public PathConfigurationExpression(LambdaExpression destinationExpression, Stack<Member> chain)
        {
            _destinationExpression = destinationExpression;
            MemberPath = new MemberPath(chain);
        }

        public MemberPath MemberPath { get; }

        public MemberInfo DestinationMember => MemberPath.Last;
        
        public void MapFrom<TSourceMember>(Expression<Func<TSource, TSourceMember>> sourceExpression)
        {
            MapFromUntyped(sourceExpression);
        }

        public void Ignore()
        {
            PathMapActions.Add(pm => pm.Ignored = true);
        }

        public void MapFromUntyped(LambdaExpression sourceExpression)
        {
            _sourceExpression = sourceExpression ?? throw new ArgumentNullException(nameof(sourceExpression), $"{nameof(sourceExpression)} may not be null when mapping {DestinationMember.Name} from {typeof(TSource)} to {typeof(TDestination)}.");
            PathMapActions.Add(pm =>
            {
                pm.CustomMapExpression = sourceExpression;
                pm.Ignored = false;
            });
        }

        public void Configure(TypeMap typeMap)
        {
            var pathMap = typeMap.FindOrCreatePathMapFor(_destinationExpression, MemberPath, typeMap);

            Apply(pathMap);
        }

        private void Apply(PathMap pathMap)
        {
            foreach (var action in PathMapActions)
            {
                action(pathMap);
            }
        }

        internal static IPropertyMapConfiguration Create(LambdaExpression destination, LambdaExpression source)
        {
            if (destination == null || !destination.IsMemberPath(out var chain))
            {
                return null;
            }
            var reversed = new PathConfigurationExpression<TSource, TDestination, object>(destination, chain);
            if (reversed.MemberPath.Length == 1)
            {
                var reversedMemberExpression = new MemberConfigurationExpression<TSource, TDestination, object>(reversed.DestinationMember, typeof(TSource));
                reversedMemberExpression.MapFromUntyped(source);
                return reversedMemberExpression;
            }
            reversed.MapFromUntyped(source);
            return reversed;
        }

        public LambdaExpression SourceExpression => _sourceExpression;
        public LambdaExpression GetDestinationExpression() => _destinationExpression;
        public IPropertyMapConfiguration Reverse() => Create(_sourceExpression, _destinationExpression);

        public void Condition(Func<ConditionParameters<TSource, TDestination, TMember>, bool> condition)
        {
            PathMapActions.Add(pm =>
            {
                Expression<Func<TSource, TDestination, TMember, TMember, ResolutionContext, bool>> expr =
                    (src, dest, srcMember, destMember, ctxt) =>
                        condition(new ConditionParameters<TSource, TDestination, TMember>(src, dest, srcMember, destMember, ctxt));
                pm.Condition = expr;
            });
        }
    }
}