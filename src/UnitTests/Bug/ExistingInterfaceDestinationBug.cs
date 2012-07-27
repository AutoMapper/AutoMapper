using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    namespace ExistingInterfaceDestinationBug
    {
        public class DataSource : IHaveData
        {
            public string SomeData { get; set; }
        }

        public class DataDestination : INeedData
        {
            public string SomeData { get; set; }
        }

        public interface IHaveData
        {
            string SomeData { get; }
        }

        public interface INeedData
        {
            string SomeData { set; }
        }

        [TestFixture]
        public class ExistingInterfaceDestinationBug : AutoMapperSpecBase
        {
            [Test]
            public void Should_map_to_existing_object()
            {
                Mapper.CreateMap<IHaveData, INeedData>();
                Mapper.AssertConfigurationIsValid();

                var source = new DataSource();
                source.SomeData = "Foo";

                var destination = new DataDestination();

                Mapper.Map<IHaveData, INeedData>(source, destination);

                destination.SomeData.ShouldEqual(source.SomeData);
            }
        }
    }
}
