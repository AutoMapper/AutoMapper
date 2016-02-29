namespace AutoMapper.Execution
{
    public class NullReplacementMethod : IValueResolver
    {
        private readonly object _nullSubstitute;

        public NullReplacementMethod(object nullSubstitute)
        {
            _nullSubstitute = nullSubstitute;
        }

        public object Resolve(object source, ResolutionContext context)
        {
            if (_nullSubstitute == null)
            {
                return source;
            }
            return source ?? _nullSubstitute;
        }
    }
}