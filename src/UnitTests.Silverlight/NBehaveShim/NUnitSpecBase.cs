using NUnit.Framework;

namespace NBehave.Spec.NUnit
{
    [TestFixture]
    public abstract class SpecBase : Spec.SpecBase
    {
        [TestFixtureSetUp]
        public override void MainSetup()
        {
            base.MainSetup();
        }

        [TestFixtureTearDown]
        public override void MainTeardown()
        {
            base.MainTeardown();
        }
    }
}
