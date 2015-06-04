namespace AutoMapper
{
    using System;

    public class NullReferenceExceptionSwallowingResolver : IMemberResolver
    {
        private readonly IMemberResolver _inner;

        public NullReferenceExceptionSwallowingResolver(IMemberResolver inner)
        {
            _inner = inner;
        }

        public ResolutionResult Resolve(ResolutionResult source)
        {
            try
            {
                return _inner.Resolve(source);
            }
            catch (NullReferenceException)
            {
                return source.New(null, MemberType);
            }
        }

        public Type MemberType => _inner.MemberType;
    }
}