namespace AutoMapper
{
	internal class DefaultResolver : IValueResolver
	{
		public ResolutionResult Resolve(ResolutionResult source)
		{
			return new ResolutionResult(source.Value);
		}
	}
}