using NBehave.Spec.NUnit;

namespace AutoMapper.UnitTests
{
	public class AutoMapperSpecBase : SpecBase
	{
		protected override void Cleanup()
		{
			AutoMapper.Reset();
		}
	}
}