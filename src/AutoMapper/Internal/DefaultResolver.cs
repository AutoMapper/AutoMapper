namespace AutoMapper
{
	internal class DefaultResolver : IValueResolver
	{
		public ResolutionResult Resolve(ResolutionResult source)
		{
			return source.New(source.Value);
		}
	}
}