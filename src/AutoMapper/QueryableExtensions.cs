namespace AutoMapper.QueryableExtensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using Impl;
    using Internal;

    public static class Extensions
    {
        private static readonly IDictionaryFactory DictionaryFactory = PlatformAdapter.Resolve<IDictionaryFactory>();

        private static readonly Internal.IDictionary<ExpressionRequest, LambdaExpression> _expressionCache
            = DictionaryFactory.CreateDictionary<ExpressionRequest, LambdaExpression>();

        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <param name="membersToExpand">Expand members explicitly previously marked as members to explicitly expand</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(
            this IMappingEngine mappingEngine, params string[] membersToExpand)
        {
            return (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new ExpressionRequest(typeof(TSource), typeof(TDestination), membersToExpand),
                    tp => CreateMapExpression(mappingEngine, tp, DictionaryFactory.CreateDictionary<ExpressionRequest, int>()));
        }


        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property.
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source)
        {
            return source.Project(Mapper.Engine);
        }

        /// <summary>
        /// Extention method to project from a queryable using the provided mapping engine
        /// Due to generic parameter inference, you need to call Project().To to execute the map
        /// </summary>
        /// <remarks>Projections are only calculated once and cached</remarks>
        /// <typeparam name="TSource">Source type</typeparam>
        /// <param name="source">Queryable source</param>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression to project into</returns>
        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression<TSource>(source, mappingEngine);
        }

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request,
            Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(request.SourceType, "dto");

            var total = CreateMapExpression(mappingEngine, request, instanceParameter, typePairCount);

            return Expression.Lambda(total, instanceParameter);
        }

        private static Expression CreateMapExpression(IMappingEngine mappingEngine, ExpressionRequest request, Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(request.SourceType,
                request.DestinationType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, request.SourceType.Name, request.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, request, typeMap, instanceParameter, typePairCount);

            var expression = Expression.MemberInit(
                Expression.New(request.DestinationType),
                bindings.ToArray()
                );

            return expression;
        }

        private static List<MemberBinding> CreateMemberBindings(IMappingEngine mappingEngine, ExpressionRequest request,
            TypeMap typeMap,
            Expression instanceParameter, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            var bindings = new List<MemberBinding>();

            var visitCount = typePairCount.AddOrUpdate(request, 0, (tp, i) => i + 1);

            if (visitCount >= typeMap.MaxDepth)
                return bindings;

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = ResolveExpression(propertyMap, request.SourceType, instanceParameter);

                if (propertyMap.ExplicitExpansion && !request.IncludedMembers.Contains(propertyMap.DestinationProperty.Name))
                    continue;

                var propertyTypeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(result.Type, propertyMap.DestinationPropertyType);
                var propertyRequest = new ExpressionRequest(result.Type, propertyMap.DestinationPropertyType, request.IncludedMembers);
                
                MemberAssignment bindExpression;

                if (propertyMap.DestinationPropertyType.IsNullableType()
                    && !result.Type.IsNullableType())
                {
                    bindExpression = BindNullableExpression(propertyMap, result);
                }
                else if (propertyMap.DestinationPropertyType.IsAssignableFrom(result.Type))
                {
                    bindExpression = BindAssignableExpression(propertyMap, result);
                }
                else if (propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    propertyMap.DestinationPropertyType != typeof(string))
                {
                    bindExpression = BindEnumerableExpression(mappingEngine, propertyMap, request, result, typePairCount);
                }
                else if (propertyTypeMap != null && propertyTypeMap.CustomProjection == null)
                {
                    bindExpression = BindMappedTypeExpression(mappingEngine, propertyMap, propertyRequest, result, typePairCount);
                }
                else if (propertyTypeMap != null && propertyTypeMap.CustomProjection != null)
                {
                    bindExpression = BindCustomProjectionExpression(propertyMap, propertyTypeMap, result);
                }
                else
                {
                    throw new AutoMapperMappingException("Unable to create a map expression from " + result.Type + " to " + propertyMap.DestinationPropertyType);
                }

                bindings.Add(bindExpression);
            }
            return bindings;
        }

        private static MemberAssignment BindCustomProjectionExpression(PropertyMap propertyMap, TypeMap propertyTypeMap, ExpressionResolutionResult result)
        {
            var visitor = new ParameterReplacementVisitor(result.ResolutionExpression);

            var replaced = visitor.Visit(propertyTypeMap.CustomProjection.Body);

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, replaced);
        }

        private static MemberAssignment BindNullableExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
        {
            if (result.ResolutionExpression.NodeType == ExpressionType.MemberAccess
                && ((MemberExpression)result.ResolutionExpression).Expression.NodeType == ExpressionType.MemberAccess)
            {
                var destType = propertyMap.DestinationPropertyType;
                var memberExpr = (MemberExpression)result.ResolutionExpression;
                var parentExpr = memberExpr.Expression;
                Expression expressionToBind = Expression.Convert(memberExpr, destType);
                var nullExpression = Expression.Convert(Expression.Constant(null), destType);
                while (parentExpr.NodeType != ExpressionType.Parameter)
                {
                    memberExpr = (MemberExpression)memberExpr.Expression;
                    parentExpr = memberExpr.Expression;
                    expressionToBind = Expression.Condition(
                        Expression.Equal(memberExpr, Expression.Constant(null)),
                        nullExpression,
                        expressionToBind
                        );
                }

                return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, expressionToBind);
            }

            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, Expression.Convert(result.ResolutionExpression, propertyMap.DestinationPropertyType));
        }

        private static MemberAssignment BindMappedTypeExpression(IMappingEngine mappingEngine, PropertyMap propertyMap, ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            MemberAssignment bindExpression;
            var transformedExpression = CreateMapExpression(mappingEngine, request,
                result.ResolutionExpression,
                typePairCount);

            // Handles null source property so it will not create an object with possible non-nullable propeerties 
            // which would result in an exception.
            if (mappingEngine.ConfigurationProvider.MapNullSourceValuesAsNull)
            {
                var expressionNull = Expression.Constant(null, propertyMap.DestinationPropertyType);
                transformedExpression =
                    Expression.Condition(Expression.NotEqual(result.ResolutionExpression, Expression.Constant(null)),
                        transformedExpression, expressionNull);
            }

            bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, transformedExpression);
            return bindExpression;
        }

        private static MemberAssignment BindAssignableExpression(PropertyMap propertyMap, ExpressionResolutionResult result)
        {
            return Expression.Bind(propertyMap.DestinationProperty.MemberInfo, result.ResolutionExpression);
        }

        private static MemberAssignment BindEnumerableExpression(IMappingEngine mappingEngine, PropertyMap propertyMap,  ExpressionRequest request, ExpressionResolutionResult result, Internal.IDictionary<ExpressionRequest, int> typePairCount)
        {
            MemberAssignment bindExpression;
            Type destinationListType = GetDestinationListTypeFor(propertyMap);
            Type sourceListType = null;
            // is list

            if (result.Type.IsArray)
            {
                sourceListType = result.Type.GetElementType();
            }
            else
            {
                sourceListType = result.Type.GetGenericArguments().First();
            }
            var listTypePair = new ExpressionRequest(sourceListType, destinationListType, request.IncludedMembers);


            var selectExpression = result.ResolutionExpression;
            if (sourceListType != destinationListType)
            {
                var transformedExpression = CreateMapExpression(mappingEngine, listTypePair, typePairCount);
                selectExpression = Expression.Call(
                    typeof (Enumerable),
                    "Select",
                    new[] {sourceListType, destinationListType},
                    result.ResolutionExpression,
                    transformedExpression);
            }

            if (typeof (IList<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType)
                || typeof(ICollection<>).MakeGenericType(destinationListType).IsAssignableFrom(propertyMap.DestinationPropertyType))
            {
                // Call .ToList() on IEnumerable
                var toListCallExpression = GetToListCallExpression(propertyMap, destinationListType, selectExpression);

                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toListCallExpression);
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                MethodCallExpression toArrayCallExpression = Expression.Call(
                    typeof(Enumerable),
                    "ToArray",
                    new Type[] { destinationListType },
                    selectExpression);
                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, toArrayCallExpression);
            }           
            else
            {
                // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                bindExpression = Expression.Bind(propertyMap.DestinationProperty.MemberInfo, selectExpression);
            }
            return bindExpression;
        }

        private static Type GetDestinationListTypeFor(PropertyMap propertyMap)
        {
            Type destinationListType;
            if (propertyMap.DestinationPropertyType.IsArray)
                destinationListType = propertyMap.DestinationPropertyType.GetElementType();
            else
                destinationListType = propertyMap.DestinationPropertyType.GetGenericArguments().First();
            return destinationListType;
        }

        private static MethodCallExpression GetToListCallExpression(PropertyMap propertyMap, Type destinationListType,
            Expression selectExpression)
        {
            return Expression.Call(
                typeof(Enumerable),
                propertyMap.DestinationPropertyType.IsArray ? "ToArray" : "ToList",
                new[] { destinationListType },
                selectExpression);
        }

        private static ExpressionResolutionResult ResolveExpression(PropertyMap propertyMap, Type currentType, Expression instanceParameter)
        {
            Expression currentChild = instanceParameter;
            Type currentChildType = currentType;
            foreach (var resolver in propertyMap.GetSourceValueResolvers())
            {
                var getter = resolver as IMemberGetter;
                if (getter != null)
                {
                    var memberInfo = getter.MemberInfo;

                    var propertyInfo = memberInfo as PropertyInfo;
                    if (propertyInfo != null)
                    {
                        currentChild = Expression.Property(currentChild, propertyInfo);
                        currentChildType = propertyInfo.PropertyType;
                    }
                    else
                        currentChildType = currentChild.Type;
                }
                else
                {
                    var oldParameter = propertyMap.CustomExpression.Parameters.Single();
                    var newParameter = instanceParameter;
                    var converter = new ConversionVisitor(newParameter, oldParameter);

                    currentChild = converter.Visit(propertyMap.CustomExpression.Body);
                    currentChildType = currentChild.Type;
                }
            }

            return new ExpressionResolutionResult(currentChild, currentChildType);
        }

        /// <summary>
        /// This expression visitor will replace an input parameter by another one
        /// 
        /// see http://stackoverflow.com/questions/4601844/expression-tree-copy-or-convert
        /// </summary>
        private class ConversionVisitor : ExpressionVisitor
        {
            private readonly Expression newParameter;
            private readonly ParameterExpression oldParameter;

            public ConversionVisitor(Expression newParameter, ParameterExpression oldParameter)
            {
                this.newParameter = newParameter;
                this.oldParameter = oldParameter;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                // replace all old param references with new ones
                return node.Type == oldParameter.Type ? newParameter : node;
            }

            protected override Expression VisitMember(MemberExpression node)
            {
                if (node.Expression != oldParameter) // if instance is not old parameter - do nothing
                    return base.VisitMember(node);

                var newObj = Visit(node.Expression);
                var newMember = newParameter.Type.GetMember(node.Member.Name).First();
                return Expression.MakeMemberAccess(newObj, newMember);
            }
        }

        private class ParameterReplacementVisitor : ExpressionVisitor
        {
            private readonly Expression _memberExpression;

            public ParameterReplacementVisitor(Expression memberExpression)
            {
                _memberExpression = memberExpression;
            }

            protected override Expression VisitParameter(ParameterExpression node)
            {
                return _memberExpression;
                //return base.VisitParameter(node);
            }
        }
        private class ExpressionResolutionResult
        {
            public Expression ResolutionExpression { get; private set; }
            public Type Type { get; private set; }

            public ExpressionResolutionResult(Expression resolutionExpression, Type type)
            {
                ResolutionExpression = resolutionExpression;
                Type = type;
            }
        }

        private class ExpressionRequest : IEquatable<ExpressionRequest>
        {
            private string _membersForComparison;
            public Type SourceType { get; private set; }
            public Type DestinationType { get; private set; }
            public string[] IncludedMembers { get; private set; }

            public ExpressionRequest(Type sourceType, Type destinationType, params string[] includedMembers)
            {
                SourceType = sourceType;
                DestinationType = destinationType;
                IncludedMembers = includedMembers;
                _membersForComparison = includedMembers.Distinct()
                    .OrderBy(s => s)
                    .Aggregate(string.Empty, (prev, curr) => prev + curr);
            }

            public bool Equals(ExpressionRequest other)
            {
                if (ReferenceEquals(null, other)) return false;
                if (ReferenceEquals(this, other)) return true;
                return string.Equals(_membersForComparison, other._membersForComparison) && SourceType.Equals(other.SourceType) && DestinationType.Equals(other.DestinationType);
            }

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(null, obj)) return false;
                if (ReferenceEquals(this, obj)) return true;
                if (obj.GetType() != this.GetType()) return false;
                return Equals((ExpressionRequest) obj);
            }

            public override int GetHashCode()
            {
                unchecked
                {
                    var hashCode = _membersForComparison.GetHashCode();
                    hashCode = (hashCode*397) ^ SourceType.GetHashCode();
                    hashCode = (hashCode*397) ^ DestinationType.GetHashCode();
                    return hashCode;
                }
            }

            public static bool operator ==(ExpressionRequest left, ExpressionRequest right)
            {
                return Equals(left, right);
            }

            public static bool operator !=(ExpressionRequest left, ExpressionRequest right)
            {
                return !Equals(left, right);
            }
        }

    }

    /// <summary>
    /// Continuation to execute projection
    /// </summary>
    public interface IProjectionExpression
    {
        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TResult">Destination type to map to</typeparam>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TResult> To<TResult>();

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TResult">Destination type to map to</typeparam>
        /// <param name="membersToExpand">Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TResult> To<TResult>(params string[] membersToExpand);

        /// <summary>
        /// Projects the source type to the destination type given the mapping configuration
        /// </summary>
        /// <typeparam name="TResult">Destination type to map to</typeparam>
        /// <param name="membersToExpand">>Explicit members to expand</param>
        /// <returns>Queryable result, use queryable extension methods to project and execute result</returns>
        IQueryable<TResult> To<TResult>(params Expression<Func<TResult, object>>[] membersToExpand);
    }

    public class ProjectionExpression<TSource> : IProjectionExpression
    {
        private readonly IQueryable<TSource> _source;
        private readonly IMappingEngine _mappingEngine;

        public ProjectionExpression(IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            _source = source;
            _mappingEngine = mappingEngine;
        }

        public IQueryable<TResult> To<TResult>()
        {
            return To<TResult>(new string[0]);
        }

        public IQueryable<TResult> To<TResult>(params string[] membersToExpand)
        {
            Expression<Func<TSource, TResult>> expr = _mappingEngine.CreateMapExpression<TSource, TResult>(membersToExpand);

            return _source.Select(expr);
        }

        public IQueryable<TResult> To<TResult>(params Expression<Func<TResult, object>>[] membersToExpand)
        {
            var members = membersToExpand.Select(expr =>
            {
                var visitor = new MemberVisitor();
                visitor.Visit(expr);
                return visitor.MemberName;
            })
                .ToArray();
            return To<TResult>(members);
        }

        private class MemberVisitor : ExpressionVisitor
        {
            protected override Expression VisitMember(MemberExpression node)
            {
                MemberName = node.Member.Name;
                return base.VisitMember(node);
            }

            public string MemberName { get; private set; }
        }
    }
}
