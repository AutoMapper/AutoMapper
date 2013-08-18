using System;

namespace AutoMapper
{
	public interface IValueResolver
	{
		ResolutionResult Resolve(ResolutionResult source);
	}
}