using AutoMapper.Internal;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
namespace AutoMapper
{
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConstructorMap
    {
        private bool? _canResolve;
        private readonly List<ConstructorParameterMap> _ctorParams = new List<ConstructorParameterMap>();
        public ConstructorInfo Ctor { get; }
        public TypeMap TypeMap { get; }
        public IEnumerable<ConstructorParameterMap> CtorParams => _ctorParams;
        public ConstructorMap(ConstructorInfo ctor, TypeMap typeMap)
        {
            Ctor = ctor;
            TypeMap = typeMap;
        }
        public bool CanResolve
        {
            get => _canResolve ??= ParametersCanResolve();
            set => _canResolve = value;
        }
        private bool ParametersCanResolve()
        {
            foreach (var param in _ctorParams)
            {
                if (!param.CanResolveValue)
                {
                    return false;
                }
            }
            return true;
        }
        public void AddParameter(ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers, bool canResolve) =>
            _ctorParams.Add(new ConstructorParameterMap(TypeMap, parameter, sourceMembers, canResolve));
    }
    [EditorBrowsable(EditorBrowsableState.Never)]
    public class ConstructorParameterMap : MemberMap
    {
        private readonly MemberInfo[] _sourceMembers;
        private Type _sourceType;
        public ConstructorParameterMap(TypeMap typeMap, ParameterInfo parameter, IEnumerable<MemberInfo> sourceMembers, bool canResolveValue)
        {
            TypeMap = typeMap;
            Parameter = parameter;
            _sourceMembers = sourceMembers.ToArray();
            CanResolveValue = canResolveValue;
        }
        public ParameterInfo Parameter { get; }
        public override TypeMap TypeMap { get; }
        public override Type SourceType
        {
            get => _sourceType ??=
                CustomMapExpression?.ReturnType ??
                CustomMapFunction?.ReturnType ??
                (_sourceMembers.Length > 0 ? _sourceMembers[_sourceMembers.Length - 1].GetMemberType() : Parameter.ParameterType);
            protected set => _sourceType = value;
        }
        public override Type DestinationType => Parameter.ParameterType;
        public override MemberInfo[] SourceMembers => _sourceMembers;
        public override string DestinationName => Parameter.Name;
        public bool HasDefaultValue => Parameter.IsOptional;
        public override LambdaExpression CustomMapExpression { get; set; }
        public override LambdaExpression CustomMapFunction { get; set; }
        public override bool CanResolveValue { get; set; }
        public override bool Inline { get; set; }
        public Expression DefaultValue() => Parameter.GetDefaultValue();
        public override string ToString() => Parameter.Member.DeclaringType + "." + Parameter.Member + ".parameter " + Parameter.Name;
    }
}