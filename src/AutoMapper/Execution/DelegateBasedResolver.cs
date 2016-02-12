namespace AutoMapper.Execution
{
    using System;

    public class DelegateBasedResolver<TSource, TMember> : IMemberResolver
    {
        private readonly Func<ResolutionResult, TMember> _method;

        public DelegateBasedResolver(Func<TSource, TMember> method) : this(r=>method((TSource)r.Value))
        {
        }

        public DelegateBasedResolver(Func<ResolutionResult, TMember> method)
        {
            _method = method;
        }

        public Type MemberType => typeof(TMember);

        public ResolutionResult Resolve(ResolutionResult source)
        {
            if(source.Value != null && !(source.Value is TSource))
            {
                throw new ArgumentException($"Expected obj to be of type {typeof (TSource)} but was {source.Value.GetType()}");
            }
            var result = _method(source);
            return source.New(result, typeof(TMember));
        }
    }
}