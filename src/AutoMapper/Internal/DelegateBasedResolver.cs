namespace AutoMapper.Internal
{
    using System;

    public class DelegateBasedResolver<TSource> : IValueResolver
    {
        private readonly Func<ResolutionResult, object> _method;

        public DelegateBasedResolver(Func<ResolutionResult, object> method)
        {
            _method = method;
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && !(source.Value is TSource))
            {
                throw new ArgumentException("Expected obj to be of type " + typeof (TSource) + " but was " +
                                            source.Value.GetType());
            }

            var result = _method(source);

            return source.New(result);
        }
    }

    public class DelegateBasedResolver<TSource, TMember> : IMemberResolver
    {
        private readonly Func<TSource, TMember> _method;

        public DelegateBasedResolver(Func<TSource, TMember> method)
        {
            _method = method;
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if (source.Value != null && !(source.Value is TSource))
            {
                throw new ArgumentException("Expected obj to be of type " + typeof(TSource) + " but was " +
                                            source.Value.GetType());
            }

            var result = _method((TSource)source.Value);

            return source.New(result, MemberType);
        }

        public Type MemberType => typeof(TMember);
    }
}