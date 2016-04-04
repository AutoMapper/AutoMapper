using AutoMapper.QueryableExtensions;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    public class ConstructorMap
    {
        private bool _sealed;
        private readonly IList<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();
        private Func<ResolutionContext, object> _ctorFunc;

        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        internal IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;

        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }

        private static readonly IExpressionResultConverter[] ExpressionResultConverters =
        {
            new MemberGetterExpressionResultConverter(),
            new MemberResolverExpressionResultConverter(),
        };

        private Expression<Func<object, ResolutionContext, object>> _ctorExpr;

        public bool CanResolve => CtorParams.All(param => param.CanResolve);

        public Expression NewExpression(Expression instanceParameter)
        {
            var parameters = CtorParams.Select(map =>
            {
                var result = new ExpressionResolutionResult(instanceParameter, Ctor.DeclaringType);

                var matchingExpressionConverter =
                    ExpressionResultConverters.FirstOrDefault(c => c.CanGetExpressionResolutionResult(result, map));

                if (matchingExpressionConverter == null)
                    throw new Exception("Can't resolve this to Queryable Expression");

                result = matchingExpressionConverter.GetExpressionResolutionResult(result, map);

                return result;
            });
            return Expression.New(Ctor, parameters.Select(p => p.ResolutionExpression));
        }

        internal void Seal(TypeMapRegistry typeMapRegistry)
        {
            if (_sealed)
                return;

            if (!CanResolve)
                return;

            var srcParam = Expression.Parameter(typeof(object), "src");
            var source = Expression.Convert(srcParam, TypeMap.SourceType);
            var ctxtParam = Expression.Parameter(typeof(ResolutionContext), "ctxt");

            var ctorArgs = CtorParams.Select(p => p.CreateExpression(typeMapRegistry).ReplaceParameters(source, ctxtParam));

            ctorArgs =
                ctorArgs.Zip(Ctor.GetParameters(),
                    (exp, pi) => exp.Type == pi.ParameterType ? exp : Expression.Convert(exp, pi.ParameterType))
                    .ToArray();

            var newExpr = Expression.New(Ctor, ctorArgs);

            _ctorExpr = Expression.Lambda<Func<object, ResolutionContext, object>>(newExpr, srcParam, ctxtParam);

            var ctorFunc = _ctorExpr.Compile();

            _ctorFunc = ctxt => BuildDest(ctorFunc, ctxt);

            _sealed = true;
        }

        public object ResolveValue(ResolutionContext context)
        {
            return _ctorFunc(context);
        }

        private object BuildDest(Func<object, ResolutionContext, object> ctorFunc, ResolutionContext ctxt)
        {
            return ctorFunc(ctxt.SourceValue, ctxt);
        }

        //private object Resolve(ResolutionContext context, ConstructorParameterMap map)
        //{

        //    var result = map.ResolveValue(context);

        //    var sourceType = result?.GetType() ?? context.SourceType;
        //    var destinationType = map.Parameter.ParameterType;

        //    var typeMap = context.ConfigurationProvider.ResolveTypeMap(sourceType, context.SourceType, destinationType);

        //    if(typeMap == null && map.Parameter.IsOptional)
        //    {
        //        object value = map.Parameter.DefaultValue;
        //        return value;
        //    }
        //    else
        //    {
        //        var value = context.Mapper.Map(result, null, sourceType, destinationType, context);
        //        return value;
        //    }
        //}
        public void AddParameter(ParameterInfo parameter, IMemberGetter[] resolvers, bool canResolve)
        {
            _ctorParams.Add(new ConstructorParameterMap(this, parameter, resolvers, canResolve));
        }
    }
}