namespace AutoMapper.Internal
{
    using System;

    public class DeferredInstantiatedResolver : IValueResolver
    {
        private readonly Func<ResolutionContext, IValueResolver> _constructor;

        public DeferredInstantiatedResolver(Func<ResolutionContext, IValueResolver> constructor)
        {
            _constructor = constructor;
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            var resolver = _constructor(source.Context);

            return resolver.Resolve(source);
        }
    }
}