namespace AutoMapper.Execution
{
    using System;

    public class DeferredInstantiatedResolver : IValueResolver
    {
        private readonly Func<ResolutionContext, IValueResolver> _constructor;

        public DeferredInstantiatedResolver(Func<ResolutionContext, IValueResolver> constructor)
        {
            _constructor = constructor;
        }

        public object Resolve(object source, ResolutionContext context)
        {
            var resolver = _constructor(context);

            return resolver.Resolve(source, context);
        }
    }
}