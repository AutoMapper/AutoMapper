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

        public object Resolve(object source, ResolutionContext context)
        {
            try
            {
                return _inner.Resolve(source, context);
            }
            catch (NullReferenceException)
            {
                return null;
            }
        }

        public Type MemberType => _inner.MemberType;
    }
}