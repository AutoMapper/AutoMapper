namespace AutoMapper.Mappers
{
    using System.Linq;
    using System.Linq.Expressions;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class ExpressionMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            var sourceDelegateType = context.SourceType.GetGenericArguments()[0];
            var destDelegateType = context.DestinationType.GetGenericArguments()[0];
            var expression = (LambdaExpression) context.SourceValue;

            if (sourceDelegateType.GetGenericTypeDefinition() != destDelegateType.GetGenericTypeDefinition())
                throw new AutoMapperMappingException("Source and destination expressions must be of the same type.");

            var parameters = expression.Parameters.ToArray();
            var body = expression.Body;

            for (int i = 0; i < expression.Parameters.Count; i++)
            {
                var sourceParamType = sourceDelegateType.GetGenericArguments()[i];
                var destParamType = destDelegateType.GetGenericArguments()[i];

                if (sourceParamType == destParamType)
                    continue;

                var typeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(destParamType, sourceParamType);

                if (typeMap == null)
                    throw new AutoMapperMappingException(
                        $"Could not find type map from destination type {destParamType} to source type {sourceParamType}. Use CreateMap to create a map from the source to destination types.");

                var oldParam = expression.Parameters[i];
                var newParam = Expression.Parameter(typeMap.SourceType, oldParam.Name);
                parameters[i] = newParam;
                var visitor = new MappingVisitor(typeMap, oldParam, newParam);
                body = visitor.Visit(body);
            }
            return Expression.Lambda(body, parameters);
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return typeof (LambdaExpression).IsAssignableFrom(context.SourceType)
                   && context.SourceType != typeof (LambdaExpression)
                   && typeof (LambdaExpression).IsAssignableFrom(context.DestinationType)
                   && context.DestinationType != typeof (LambdaExpression);
        }

        /// <summary>
        /// 
        /// </summary>
        private class MappingVisitor : ExpressionVisitor
        {
            private readonly TypeMap _typeMap;
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="typeMap"></param>
            /// <param name="oldParam"></param>
            /// <param name="newParam"></param>
            public MappingVisitor(TypeMap typeMap, ParameterExpression oldParam, ParameterExpression newParam)
            {
                _typeMap = typeMap;
                _oldParam = oldParam;
                _newParam = newParam;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (ReferenceEquals(node, _oldParam))
                    return _newParam;
                return node;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitMember(MemberExpression node)
            {
                var memberAccessor = node.Member.ToMemberAccessor();
                var propertyMap = _typeMap.GetExistingPropertyMapFor(memberAccessor);

                if (propertyMap.CustomExpression != null)
                {
                    var replaced = new ParameterReplacementVisitor(_newParam);
                    var newBody = replaced.Visit(propertyMap.CustomExpression.Body);
                    return newBody;
                }

                var replacedExpression = Visit(node.Expression);

                return propertyMap.GetSourceValueResolvers()
                    .OfType<IMemberGetter>()
                    .Aggregate(replacedExpression,
                        (current, memberGetter) => Expression.MakeMemberAccess(current, memberGetter.MemberInfo));
            }
        }

        /// <summary>
        /// 
        /// </summary>
        private class ParameterReplacementVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression _newParam;

            /// <summary>
            /// 
            /// </summary>
            /// <param name="newParam"></param>
            public ParameterReplacementVisitor(ParameterExpression newParam)
            {
                _newParam = newParam;
            }

            /// <summary>
            /// 
            /// </summary>
            /// <param name="node"></param>
            /// <returns></returns>
            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _newParam;
            }
        }
    }
}