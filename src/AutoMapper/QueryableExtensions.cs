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

        public static Expression<Func<TSource, TDestination>> CreateMapExpression<TSource, TDestination>(this IMappingEngine mappingEngine)
        {
            return (Expression<Func<TSource, TDestination>>)
                _expressionCache.GetOrAdd(new TypePair(typeof(TSource), typeof(TDestination)), tp =>
                {
                    return CreateMapExpression(mappingEngine, tp.SourceType, tp.DestinationType);
                });
        }



        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source)
        {
            return source.Project(Mapper.Engine);
        }

        public static IProjectionExpression Project<TSource>(
            this IQueryable<TSource> source, IMappingEngine mappingEngine)
        {
            return new ProjectionExpression<TSource>(source, mappingEngine);
        }

        private static LambdaExpression CreateMapExpression(IMappingEngine mappingEngine, Type typeIn, Type typeOut)
        {
            var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(typeIn, typeOut);

            // this is the input parameter of this expression with name <variableName>
            ParameterExpression instanceParameter = Expression.Parameter(typeIn, "dto");

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
                            ((LambdaExpression)propertyMap.CustomExpression).Parameters.Single();
                        var newParameter = instanceParameter;
                        var converter = new ConversionVisitor(newParameter, oldParameter);
                        currentChild = converter.Visit(((LambdaExpression)propertyMap.CustomExpression).Body);
                        var propertyInfo = propertyMap.SourceMember as PropertyInfo;
                        
                        if (propertyInfo != null)
                        {
                            currentChildType = propertyInfo.PropertyType;
                        }
                        else if (propertyMap.CustomExpression != null &&
                                 propertyMap.CustomExpression.Body.NodeType == ExpressionType.Call &&
                                 (((MethodCallExpression)propertyMap.CustomExpression.Body).Method.Name == "OrderBy") ||
                                 (((MethodCallExpression)propertyMap.CustomExpression.Body).Method.Name == "ThenBy"))
                        {
                            currentChildType = ((MethodCallExpression)propertyMap.CustomExpression.Body).Method.ReturnParameter.ParameterType;
                        }
                    }
                }

                var prop = destinationProperty.MemberInfo as PropertyInfo;

                // next to lists, also arrays
                // and objects!!!
                if (prop != null &&
                    prop.PropertyType.GetInterfaces().Any(t => t.Name == "IEnumerable") &&
                    prop.PropertyType != typeof(string))
                {

                    Type destinationListType = prop.PropertyType.GetGenericArguments().First();
                    Type sourceListType = null;
                    // is list

                    sourceListType = currentChildType.GetGenericArguments().First();

                    //var newVariableName = "t" + (i++);
                    var transformedExpression = CreateMapExpression(mappingEngine, sourceListType, destinationListType);

                    MethodCallExpression selectExpression = Expression.Call(
                                typeof(Enumerable),
                                "Select",
                                new[] { sourceListType, destinationListType },
                                currentChild,
                                transformedExpression);

                    if (typeof(IList).IsAssignableFrom(prop.PropertyType))
                    {
                        MethodCallExpression toListCallExpression = Expression.Call(
                            typeof(Enumerable),
                            "ToList",
                            new Type[] { destinationListType },
                            selectExpression);

                        // todo .ToArray()
                        bindings.Add(Expression.Bind(destinationMember, toListCallExpression));
                    }
                    else
                    {
                        // destination type implements ienumerable, but is not an ilist. allow deferred enumeration
                        bindings.Add(Expression.Bind(destinationMember, selectExpression));
                    }
                }
                else
                {
                    // does of course not work for subclasses etc./generic ...
                    if (currentChildType != prop.PropertyType &&
                        // avoid nullable etc.
                        prop.PropertyType.BaseType != typeof(ValueType) &&
                        prop.PropertyType.BaseType != typeof(Enum))
                    {
                        var transformedExpression = CreateMapExpression(mappingEngine, currentChildType, prop.PropertyType);
                        var expr2 = Expression.Invoke(
                            transformedExpression,
                            currentChild
                        );
                        bindings.Add(Expression.Bind(destinationMember, expr2));
                    }
                    else
                    {
                        bindings.Add(Expression.Bind(destinationMember, currentChild));
                    }
                }

            }
            Expression total;
            if (typeOut.IsAbstract)
            {
                if (typeMap.CustomMapper == null)
                    throw new AutoMapperMappingException(
                        String.Format("Abstract type {0} can not be mapped without custom mapper (tip: use ConvertUsing)", typeOut.Name));
                // We are going to return roughly following expression
                // typeOut (typeIn)x => (typeOut)(typeMap.CustomMapper.Invoke(new ResolutionContext(typeMap, x, typeIn, typeOut, options)))

                // This expression generates a new ResolutionContext
                // for the custom mapper (ResolveCore)
                var context = Expression.MemberInit(
                    Expression.New(
                        typeof(ResolutionContext).GetConstructor(new[]
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
                                typeof(Func<ResolutionContext, object>).GetMethod("Invoke"), context);
                // We have to convert the object from Invoke to typeOut
                total = Expression.Convert(invoke, typeOut);
            }
            else
                total = Expression.MemberInit(
                    Expression.New(typeOut),
                    bindings.ToArray()
                );

            return Expression.Lambda(total, instanceParameter);
        }

        /// <summary>
        /// This expression visitor will replace an input parameter by another one
        /// 
        /// see http://stackoverflow.com/questions/4601844/expression-tree-copy-or-convert
        /// </summary>
        private class ConversionVisitor : ExpressionVisitor
        {
            private readonly ParameterExpression newParameter;
            private readonly ParameterExpression oldParameter;

            public ConversionVisitor(ParameterExpression newParameter, ParameterExpression oldParameter)
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

    public interface IProjectionExpression
    {
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
