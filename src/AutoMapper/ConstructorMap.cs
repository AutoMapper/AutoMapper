using System;
using System.Collections.Generic;
using System.Reflection;
using AutoMapper.Internal;

namespace AutoMapper
{
    public class ConstructorMap
    {
        private static readonly IDelegateFactory DelegateFactory = PlatformAdapter.Resolve<IDelegateFactory>();
        private readonly LateBoundParamsCtor _runtimeCtor;
        public ConstructorInfo Ctor { get; private set; }
        public IEnumerable<ConstructorParameterMap> CtorParams { get; private set; }

        public ConstructorMap(ConstructorInfo ctor, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            Ctor = ctor;
            CtorParams = ctorParams;

            _runtimeCtor = DelegateFactory.CreateCtor(ctor, CtorParams);
        }

        public object ResolveValue(ResolutionContext context, IMappingEngineRunner mappingEngine)
        {
            var ctorArgs = new List<object>();

            foreach (var map in CtorParams)
            {
                var result = map.ResolveValue(context);

                var sourceType = result.Type;
                var destinationType = map.Parameter.ParameterType;

                var typeMap = mappingEngine.ConfigurationProvider.FindTypeMapFor(result, destinationType);

                Type targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

                var newContext = context.CreateTypeContext(typeMap, result.Value, null, targetSourceType, destinationType);

                if (typeMap == null && map.Parameter.IsOptional)
                {
                    object value = map.Parameter.DefaultValue;
                    ctorArgs.Add(value);
                }
                else
                {
                    var value = mappingEngine.Map(newContext);
                    ctorArgs.Add(value);
                }
            }

            return _runtimeCtor(ctorArgs.ToArray());
        }
    }
}
