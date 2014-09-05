namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using Impl;

    public class ExpressionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            var sourceDelegateType = context.SourceType.GetGenericArguments()[0];
            var destDelegateType = context.DestinationType.GetGenericArguments()[0];
            var expression = (LambdaExpression) context.SourceValue;

            if (sourceDelegateType.GetGenericArguments().Length != destDelegateType.GetGenericArguments().Length)
                throw new AutoMapperMappingException("Source and destination expressions must have same number of generic arguments.");

            var parameters = expression.Parameters.ToArray();
            var body = expression.Body;

            for (int i = 0; i < sourceDelegateType.GetGenericArguments().Length; i++)
            {
                var sourceParamType = sourceDelegateType.GetGenericArguments()[i];
                var destParamType = destDelegateType.GetGenericArguments()[i];

                if (sourceParamType == destParamType)
                    continue;

                var typeMap = mapper.ConfigurationProvider.FindTypeMapFor(destParamType, sourceParamType);

                if (typeMap == null)
                    throw new AutoMapperMappingException(string.Format("Could not find type map from destination type {0} to source type {1}. Use CreateMap to create a map from the source to destination types.", destParamType, sourceParamType));

                var oldParam = expression.Parameters[i];
                var newParam = Expression.Parameter(typeMap.SourceType, oldParam.Name);
                parameters[i] = newParam;
                var visitor = new MappingVisitor(typeMap, oldParam, newParam);
                body = visitor.Visit(body);
            }
            return Expression.Lambda(body, parameters);
        }

        public bool IsMatch(ResolutionContext context)
        {
            return typeof(LambdaExpression).IsAssignableFrom(context.SourceType)
                && context.SourceType != typeof(LambdaExpression)
                && typeof(LambdaExpression).IsAssignableFrom(context.DestinationType)
                && context.DestinationType != typeof(LambdaExpression);
        }

        private class MappingVisitor : ExpressionVisitor
        {
            private readonly TypeMap _typeMap;
            private readonly ParameterExpression _oldParam;
            private readonly ParameterExpression _newParam;

            public MappingVisitor(TypeMap typeMap, ParameterExpression oldParam, ParameterExpression newParam)
            {
                _typeMap = typeMap;
                _oldParam = oldParam;
                _newParam = newParam;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                if (ReferenceEquals(node, _oldParam))
                    return _newParam;
                return node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                var replacedExpression = Visit(node.Expression);

                var memberAccessor = node.Member.ToMemberAccessor();
                var propertyMap = _typeMap.GetExistingPropertyMapFor(memberAccessor);

                return propertyMap.GetSourceValueResolvers()
                    .OfType<IMemberGetter>()
                    .Aggregate(replacedExpression, (current, memberGetter) => Expression.MakeMemberAccess(current, memberGetter.MemberInfo));
            }
        }
    }
}