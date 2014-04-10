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

        private static readonly Internal.IDictionary<TypePair, LambdaExpression> _expressionCache
            = DictionaryFactory.CreateDictionary<TypePair, LambdaExpression>();

        /// <summary>
        /// Create an expression tree representing a mapping from the <typeparamref name="TSource"/> type to <typeparamref name="TDestination"/> type
        /// Includes flattening and expressions inside MapFrom member configuration
        /// </summary>
        /// <typeparam name="TSource">Source Type</typeparam>
        /// <typeparam name="TDestination">Destination Type</typeparam>
        /// <param name="mappingEngine">Mapping engine instance</param>
        /// <returns>Expression tree mapping source to destination type</returns>
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(
            this IMappingEngine mappingEngine)
        {
            return (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new TypePair(typeof(TSource), typeof(TDestination)),
                    tp => CreateMapExpression(mappingEngine, tp, DictionaryFactory.CreateDictionary<TypePair, int>()));
        }


        /// <summary>
        /// Extention method to project from a queryable using the static <see cref="Mapper.Engine"/> property
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

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, TypePair typePair,
            Internal.IDictionary<TypePair, int> typePairCount)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typePair.SourceType, "dto");

            var total = CreateMapExpression(mappingEngine, typePair, instanceParameter, typePairCount);

            return Expression.Lambda(total, instanceParameter);
        }

        private static Expression CreateMapExpression(IMappingEngine mappingEngine, TypePair typePair, Expression instanceParameter, Internal.IDictionary<TypePair, int> typePairCount)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(typePair.SourceType,
                typePair.DestinationType);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, typePair.SourceType.Name, typePair.DestinationType.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, typePair, typeMap, instanceParameter, typePairCount);

            Expression total = Expression.MemberInit(
                Expression.New(typePair.DestinationType),
                bindings.ToArray()
                );

            return total;
        }

        private static List<MemberBinding> CreateMemberBindings(IMappingEngine mappingEngine, TypePair typePair,
            TypeMap typeMap,
            Expression instanceParameter, Internal.IDictionary<TypePair, int> typePairCount)
        {
            var bindings = new List<MemberBinding>();

            var visitCount = typePairCount.AddOrUpdate(typePair, 0, (tp, i) => i + 1);

            if (visitCount >= typeMap.MaxDepth)
                return bindings;

            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var result = ResolveExpression(propertyMap, typePair.SourceType, instanceParameter);

                var destinationMember = propertyMap.DestinationProperty.MemberInfo;

                MemberAssignment bindExpression;

                if (propertyMap.DestinationPropertyType.IsNullableType()
                    && !result.Type.IsNullableType())
                {
                    bindExpression = BindNullableExpression(propertyMap, result, destinationMember);
                }
                else if (propertyMap.DestinationPropertyType.IsAssignableFrom(result.Type))
                {
                    bindExpression = BindAssignableExpression(destinationMember, result);
                }
                else if (propertyMap.DestinationPropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    propertyMap.DestinationPropertyType != typeof(string))
                {
                    bindExpression = BindEnumerableExpression(mappingEngine, propertyMap, result, destinationMember, typePairCount);
                }
                else if (result.Type != propertyMap.DestinationPropertyType &&
                    // avoid nullable etc.
                         propertyMap.DestinationPropertyType.GetBaseType() != typeof(ValueType) &&
                         propertyMap.DestinationPropertyType.GetBaseType() != typeof(Enum))
                {
                    bindExpression = BindMappedTypeExpression(mappingEngine, propertyMap, result, destinationMember, typePairCount);
                }
                else
                {
                    throw new AutoMapperMappingException("Unable to create a map expression from " + result.Type + " to " + propertyMap.DestinationPropertyType);
                }

                bindings.Add(bindExpression);
            }
            return bindings;
        }

        private static MemberAssignment BindNullableExpression(PropertyMap propertyMap, ExpressionResolutionResult result, MemberInfo destinationMember)
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

                return Expression.Bind(destinationMember, expressionToBind);
            }

            return Expression.Bind(destinationMember, Expression.Convert(result.ResolutionExpression, propertyMap.DestinationPropertyType));
        }

        private static MemberAssignment BindMappedTypeExpression(IMappingEngine mappingEngine, PropertyMap propertyMap, ExpressionResolutionResult result, MemberInfo destinationMember, Internal.IDictionary<TypePair, int> typePairCount)
        {
            MemberAssignment bindExpression;
            var memberPair = new TypePair(result.Type, propertyMap.DestinationPropertyType);
            var transformedExpression = CreateMapExpression(mappingEngine, memberPair,
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

            bindExpression = Expression.Bind(destinationMember, transformedExpression);
            return bindExpression;
        }

        private static MemberAssignment BindAssignableExpression(MemberInfo destinationMember, ExpressionResolutionResult result)
        {
            return Expression.Bind(destinationMember, result.ResolutionExpression);
        }

        private static MemberAssignment BindEnumerableExpression(IMappingEngine mappingEngine, PropertyMap propertyMap, ExpressionResolutionResult result, MemberInfo destinationMember, Internal.IDictionary<TypePair, int> typePairCount)
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
            var listTypePair = new TypePair(sourceListType, destinationListType);


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

                bindExpression = Expression.Bind(destinationMember, toListCallExpression);
            }
            else if (propertyMap.DestinationPropertyType.IsArray)
            {
                // Call .ToArray() on IEnumerable
                MethodCallExpression toArrayCallExpression = Expression.Call(
                    typeof(Enumerable),
                    "ToArray",
                    new Type[] { destinationListType },
                    selectExpression);
                bindExpression = Expression.Bind(destinationMember, toArrayCallExpression);
            }           
            else
            {
                // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                bindExpression = Expression.Bind(destinationMember, selectExpression);
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

        public class ExpressionResolutionResult
        {
            public Expression ResolutionExpression { get; private set; }
            public Type Type { get; private set; }

            public ExpressionResolutionResult(Expression resolutionExpression, Type type)
            {
                ResolutionExpression = resolutionExpression;
                Type = type;
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
            Expression<Func<TSource, TResult>> expr = _mappingEngine.CreateMapExpression<TSource, TResult>();

            return _source.Select(expr);
        }
    }

#if NETFX_CORE
    internal static class ExtensionMethods
    {
        public static PropertyInfo GetProperty(this Type type, String propertyName)
        {
            return type.GetTypeInfo().GetDeclaredProperty(propertyName);
        }

        public static MethodInfo GetMethod(this Type type, String methodName)
        {
            return type.GetTypeInfo().GetDeclaredMethod(methodName);
        }

        public static bool IsSubclassOf(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsSubclassOf(parentType);
        }

        public static bool IsAssignableFrom(this Type type, Type parentType)
        {
            return type.GetTypeInfo().IsAssignableFrom(parentType.GetTypeInfo());
        }

        public static bool IsEnum(this Type type)
        {
            return type.GetTypeInfo().IsEnum;
        }

        public static bool IsPrimitive(this Type type)
        {
            return type.GetTypeInfo().IsPrimitive;
        }

        public static Type GetBaseType(this Type type)
        {
            return type.GetTypeInfo().BaseType;
        }

        public static bool IsGenericType(this Type type)
        {
            return type.GetTypeInfo().IsGenericType;
        }

        public static Type[] GetGenericArguments(this Type type)
        {
            return type.GetTypeInfo().GenericTypeArguments;
        }

        public static object GetPropertyValue(this Object instance, string propertyValue)
        {
            return instance.GetType().GetTypeInfo().GetDeclaredProperty(propertyValue).GetValue(instance);
        }

        public static TypeInfo GetTypeInfo(this Type type)
        {
            IReflectableType reflectableType = (IReflectableType)type;
            return reflectableType.GetTypeInfo();
        }

        public static IEnumerable<MemberInfo> GetMember(this Type type, string name)
        {
            return type.GetTypeInfo().DeclaredMembers.Where(m => m.Name == name);
        }

        public static IEnumerable<Type> GetInterfaces(this Type type)
        {
            return type.GetTypeInfo().ImplementedInterfaces;
        } 
    }
#else
    internal static class ExtensionMethods
    {
        public static Type GetBaseType(this Type type)
        {
            return type.BaseType;
        }
    }
#endif
}
