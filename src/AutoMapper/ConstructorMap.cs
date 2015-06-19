namespace AutoMapper
{
    using System.Collections.Generic;
    using System.Reflection;
    using Internal;

    /// <summary>
    /// 
    /// </summary>
    public class ConstructorMap
    {
        private static readonly DelegateFactory DelegateFactory = new DelegateFactory();
        private readonly ILazy<LateBoundParamsCtor> _runtimeCtor;
        /// <summary>
        /// 
        /// </summary>
        public ConstructorInfo Ctor { get; private set; }
        /// <summary>
        /// 
        /// </summary>
        public IEnumerable<ConstructorParameterMap> CtorParams { get; }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="ctor"></param>
        /// <param name="ctorParams"></param>
        public ConstructorMap(ConstructorInfo ctor, IEnumerable<ConstructorParameterMap> ctorParams)
        {
            Ctor = ctor;
            CtorParams = ctorParams;

            _runtimeCtor = LazyFactory.Create(() => DelegateFactory.CreateCtor(ctor, CtorParams));
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object ResolveValue(ResolutionContext context)
        {
            var runner = context.MapperContext.Runner;

            var ctorArgs = new List<object>();

            foreach (var map in CtorParams)
            {
                var result = map.ResolveValue(context);

                var sourceType = result.Type;
                var destinationType = map.Parameter.ParameterType;

                var typeMap = context.MapperContext.ConfigurationProvider.ResolveTypeMap(result, destinationType);

                var targetSourceType = typeMap != null ? typeMap.SourceType : sourceType;

                var newContext = context.CreateTypeContext(typeMap, result.Value, null, targetSourceType,
                    destinationType);

                if (typeMap == null && map.Parameter.IsOptional)
                {
                    var value = map.Parameter.DefaultValue;
                    ctorArgs.Add(value);
                }
                else
                {
                    var value = runner.Map(newContext);
                    ctorArgs.Add(value);
                }
            }

            return _runtimeCtor.Value(ctorArgs.ToArray());
        }
    }
}