using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class When_mapping_multiple_maps_off_a_base_but_not_including_them_all : AutoMapperSpecBase
    {
        [Fact]
        public void Should_ignore_inheritance_from_unincluded_types()
        {
            Mapper.CreateMap<A, A1>().Include<B, B1>();
            Mapper.CreateMap<B, B1>();
            Mapper.CreateMap<C, C1>();

            Mapper.Map<A, A1>(new C()).ShouldBeType<A1>();
        }

        public class A
        {
            
        }

        public class B : A
        {
            
        }

        public class C : A
        {
            
        }

        public class A1
        {

        }

        public class B1 : A1
        {

        }

        public class C1 : A1
        {

        }
    }
}