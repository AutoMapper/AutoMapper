using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    /// <summary>
    /// The purpose of these tests is to highlight unexpected behavior.
    /// The unexpected behavior is that if one explicitly declares a source type
    /// that that is the typemap which should be used.  
    /// 
    /// The current behavior uses the type returned from the .GetType() method of the instance of the source.
    /// 
    /// This behavior inhibits polymorphic behavior of classes.  
    /// Sometimes we may want to map things in different ways, and interfaces nicely allow us 
    /// to operate on an object as the interface and not on the object as a whole.
    /// </summary>
    public class TypePairWithInterfacesBugTests : AutoMapperSpecBase
    {
        interface IFoo
        {
            string Name { get; set; }
        }

        class Source : IFoo
        {
            public string Name { get; set; }
        }
        class Destination
        {
            public string Name { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
            cfg.CreateMap<IFoo, Destination>().AfterMap((s,d)=>d.Name = "IFooMack");
        });

        [Fact]
        public void Should_work_ClassTypeToClassType()
        {
            var source = new Source
            {
                Name = "BlackjacketMack"
            };
            var destination = Mapper.Map<Source, Destination>(source);

            destination.Name.ShouldEqual("BlackjacketMack");
        }

        [Fact]
        public void Should_workButDoesnt_AfterMapShouldBeCalledHere()
        {
            var source = new Source
            {
                Name = "BlackjacketMack"
            };
            var destination = Mapper.Map<IFoo, Destination>(source);

            destination.Name.ShouldEqual("IFooMack");
        }
    }
}