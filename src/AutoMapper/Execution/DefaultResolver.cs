namespace AutoMapper.Execution
{
    public class DefaultResolver : IValueResolver
    {
        public ResolutionResult Resolve(ResolutionResult source)
        {
            return source.New(source.Value);
        }
    }
}