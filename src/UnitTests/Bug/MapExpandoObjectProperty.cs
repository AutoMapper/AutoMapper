using System.Dynamic;

namespace AutoMapper.UnitTests.Bug
{
    public class MapExpandoObjectProperty : AutoMapperSpecBase
    {

        class From
        {
            public ExpandoObject ExpandoObject { get; set; }
        }

        class To
        {
            public ExpandoObject ExpandoObject { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<From, To>();
        });

        protected override void Because_of()
        {
            dynamic baseSettings = new ExpandoObject();

            var settings = Mapper.Map<To>(new From { ExpandoObject = baseSettings});
        }
    }
}