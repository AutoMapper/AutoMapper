using System;

namespace AutoMapper
{
    using AutoMapper.Execution;
    using System.Linq;
    using System.Linq.Expressions;
    using static System.Linq.Expressions.Expression;
    using static ExpressionExtensions;
    using System.Reflection;
    using Configuration;
    using Mappers;

    public class ConstructorParameterMap
    {
        public ConstructorParameterMap(ParameterInfo parameter, MemberInfo[] sourceMembers, bool canResolve)
        {
            Parameter = parameter;
            SourceMembers = sourceMembers;
            CanResolve = canResolve;
        }

        public ParameterInfo Parameter { get; }

        public MemberInfo[] SourceMembers { get; }

        public bool CanResolve { get; set; }

        public bool DefaultValue { get; private set; }

        public LambdaExpression CustomExpression { get; set; }
        public Func<object, ResolutionContext, object> CustomValueResolver { get; set; }

        public Type DestinationType => Parameter.ParameterType;

        public Expression CreateExpression(TypeMapRegistry typeMapRegistry, IConfigurationProvider configuration, ParameterExpression srcParam, ParameterExpression ctxtParam)
        {
            var valueResolverExpression = ResolveSource(srcParam, ctxtParam);
            var sourceType = valueResolverExpression.Type;
            var resolvedValue = Variable(sourceType, "resolvedValue");            
            return Block(new[] { resolvedValue },
                Assign(resolvedValue, valueResolverExpression),
                TypeMapPlanBuilder.MapExpression(typeMapRegistry, configuration, new TypePair(sourceType, DestinationType), resolvedValue, ctxtParam));
        }

        private Expression ResolveSource(ParameterExpression srcParam, ParameterExpression ctxtParam)
        {
            if(CustomExpression != null)
            {
                return CustomExpression.ConvertReplaceParameters(srcParam).IfNotNull(DestinationType);
            }
            if(CustomValueResolver != null)
            {
                // Invoking a delegate
                return Invoke(Constant(CustomValueResolver), srcParam, ctxtParam);
            }
            if(Parameter.IsOptional)
            {
                DefaultValue = true;
                return Constant(Parameter.GetDefaultValue(), Parameter.ParameterType);
            }
            return SourceMembers.Aggregate(
                            (Expression) srcParam,
                            (inner, getter) => getter is MethodInfo
                                ? Call(getter.IsStatic() ? null : inner, (MethodInfo) getter)
                                : (Expression) MakeMemberAccess(getter.IsStatic() ? null : inner, getter)
                      ).IfNotNull(DestinationType);
        }
    }
}