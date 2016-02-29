namespace AutoMapper.Execution
{
    public class DefaultResolver : IValueResolver
    {
        public object Resolve(object source, ResolutionContext context)
        {
            return source;
        }
    }
}