using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using AutoMapper.Impl;
using AutoMapper.Internal;

namespace AutoMapper.QueryableExtensions
{
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
        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(this IMappingEngine mappingEngine)
        {
            return (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new TypePair(typeof(TSource), typeof(TDestination)), tp =>
                {
                    return CreateMapExpression(mappingEngine, tp.SourceType, tp.DestinationType);
                });
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

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, Type typeIn, Type typeOut)
        {
            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typeIn, "dto");

            var total = CreateMapExpression(mappingEngine, typeIn, typeOut, instanceParameter);

            return Expression.Lambda(total, instanceParameter);
        }

        private static Expression CreateMapExpression(IMappingEngine mappingEngine, Type typeIn, Type typeOut, Expression instanceParameter)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(typeIn, typeOut);

            if (typeMap == null)
            {
                const string MessageFormat = "Missing map from {0} to {1}. Create using Mapper.CreateMap<{0}, {1}>.";

                var message = string.Format(MessageFormat, typeIn.Name, typeOut.Name);

                throw new InvalidOperationException(message);
            }

            var bindings = CreateMemberBindings(mappingEngine, typeIn, typeMap, instanceParameter);

            Expression total;
            if (typeOut.IsAbstract)
            {
                if (typeMap.CustomMapper == null)
                    throw new AutoMapperMappingException(
                        String.Format("Abstract type {0} can not be mapped without custom mapper (tip: use ConvertUsing)",
                                      typeOut.Name));
                // We are going to return roughly following expression
                // typeOut (typeIn)x => (typeOut)(typeMap.CustomMapper.Invoke(new ResolutionContext(typeMap, x, typeIn, typeOut, options)))

                // This expression generates a new ResolutionContext
                // for the custom mapper (ResolveCore)
                var context = Expression.MemberInit(
                    Expression.New(
                        typeof (ResolutionContext).GetConstructor(new[]
                        {
                            typeof (TypeMap), typeof (object),
                            typeof (Type),
                            typeof (Type),
                            typeof (MappingOperationOptions)
                        }
                            ),
                        new List<Expression>
                        {
                            Expression.Constant(typeMap),
                            instanceParameter, // Using the original parameter
                            Expression.Constant(typeIn),
                            Expression.Constant(typeOut),
                            Expression.Constant(new MappingOperationOptions())
                        })
                    );
                // This expression gets the CustomMapper from the typeMap
                Expression<Func<TypeMap, Func<ResolutionContext, object>>> method = x => x.CustomMapper;
                var customMapper = Expression.Invoke(method, Expression.Constant(typeMap));
                // This expression calls the Invoke method from the CustomMapper func
                var invoke = Expression.Call(customMapper,
                                             typeof (Func<ResolutionContext, object>).GetMethod("Invoke"), context);
                // We have to convert the object from Invoke to typeOut
                total = Expression.Convert(invoke, typeOut);
            }
            else
                total = Expression.MemberInit(
                    Expression.New(typeOut),
                    bindings.ToArray()
                    );
            return total;
        }

        private static List<MemberBinding> CreateMemberBindings(IMappingEngine mappingEngine, Type typeIn, TypeMap typeMap,
                                                 Expression instanceParameter)
        {
            var bindings = new List<MemberBinding>();
            foreach (var propertyMap in typeMap.GetPropertyMaps().Where(pm => pm.CanResolveValue()))
            {
                var destinationProperty = propertyMap.DestinationProperty;
                var destinationMember = destinationProperty.MemberInfo;

                Expression currentChild = instanceParameter;
                Type currentChildType = typeIn;
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
                    }
                    else
                    {
                        var oldParameter =
                            ((LambdaExpression) propertyMap.CustomExpression).Parameters.Single();
                        var newParameter = instanceParameter;
                        var converter = new ConversionVisitor(newParameter, oldParameter);
                        currentChild = converter.Visit(((LambdaExpression) propertyMap.CustomExpression).Body);
                        var propertyInfo = propertyMap.SourceMember as PropertyInfo;
                        if (propertyInfo != null)
                        {
                            currentChildType = propertyInfo.PropertyType;
                        }
                    }
                }

                var prop = destinationProperty.MemberInfo as PropertyInfo;

                // next to lists, also arrays
                // and objects!!!
                if (prop != null &&
                    prop.PropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    prop.PropertyType != typeof (string))
                {
                    Type destinationListType = prop.PropertyType.GetGenericArguments().First();
                    Type sourceListType = null;
                    // is list

                    sourceListType = currentChildType.GetGenericArguments().First();

                    //var newVariableName = "t" + (i++);
                    var transformedExpression = CreateMapExpression(mappingEngine, sourceListType, destinationListType);

                    MethodCallExpression selectExpression = Expression.Call(
                        typeof (Enumerable),
                        "Select",
                        new[] {sourceListType, destinationListType},
                        currentChild,
                        transformedExpression);

                    var isNullExpression = Expression.Equal(currentChild, Expression.Constant(null, currentChildType));

                    if (typeof (IList<>).MakeGenericType(destinationListType).IsAssignableFrom(prop.PropertyType))
                    {
                        MethodCallExpression toListCallExpression = Expression.Call(
                            typeof (Enumerable),
                            "ToList",
                            new Type[] {destinationListType},
                            selectExpression);

                        var toListIfSourceIsNotNull =
                            Expression.Condition(
                                isNullExpression,
                                Expression.Constant(null, toListCallExpression.Type),
                                toListCallExpression);

                        // todo .ToArray()

                        bindings.Add(Expression.Bind(destinationMember, toListIfSourceIsNotNull));
                    }
                    else
                    {
                        var selectIfSourceIsNotNull =
                            Expression.Condition(
                                isNullExpression,
                                Expression.Constant(null, selectExpression.Type),
                                selectExpression);

                        // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                        bindings.Add(Expression.Bind(destinationMember, selectIfSourceIsNotNull));
                    }
                }
                else
                {
                    // does of course not work for subclasses etc./generic ...
                    if (currentChildType != prop.PropertyType &&
                        // avoid nullable etc.
                        prop.PropertyType.BaseType != typeof (ValueType) &&
                        prop.PropertyType.BaseType != typeof (Enum))
                    {
                        var transformedExpression = CreateMapExpression(mappingEngine, currentChildType, prop.PropertyType, currentChild);

                        var isNullExpression = Expression.Equal(currentChild, Expression.Constant(null));

                        var transformIfIsNotNull =
                            Expression.Condition(isNullExpression, Expression.Constant(null, prop.PropertyType),
                                                 transformedExpression);

                        bindings.Add(Expression.Bind(destinationMember, transformIfIsNotNull));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(destinationMember, currentChild));
                    }
                }
            }
            return bindings;
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
}