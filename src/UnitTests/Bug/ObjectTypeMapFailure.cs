using NUnit.Framework;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class ObjectTypeMapFailure : NonValidatingSpecBase
    {
        [Test]
        public void Should_map_the_object_type()
        {
            var displayModel = new DisplayModel
            {
                Radius = 300
            };
            object vm = new SomeViewModel();
            Mapper.CreateMap<DisplayModel, SomeViewModel>();

            Mapper.Map(displayModel, vm);
            ((SomeViewModel)vm).Radius.ShouldEqual(300); // fails

            var vm2 = new SomeViewModel();
            Mapper.Map(displayModel, vm2);
            vm2.Radius.ShouldEqual(300); // succeeds
        }

        [Test, Ignore("Don't think this should really work - I don't know which types to create the type maps for")]
        public void Should_dynamic_map_the_object_type()
        {
            var displayModel = new DisplayModel
            {
                Radius = 300
            };
            object vm = new SomeViewModel();

            Mapper.DynamicMap(displayModel, vm);
            ((SomeViewModel)vm).Radius.ShouldEqual(300); // fails

            var vm2 = new SomeViewModel();
            Mapper.DynamicMap(displayModel, vm2);
            vm2.Radius.ShouldEqual(300); // succeeds
        }

        public class SomeViewModel
        {
            public int Radius { get; set; }
        }

        public class DisplayModel
        {
            public int Radius { get; set; }
        }
    }
}
