﻿namespace AutoMapper.UnitTests.Bug
{
    namespace SetterOnlyBug
    {
        using Should;
        using Xunit;

        public class MappingTests : AutoMapperSpecBase
        {
            protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<Source, Desitination>()
                    .ForMember("Property", o => o.Ignore());
            });

            [Fact]
            public void TestMapping()
            {
                var source = new Source
                {
                    Property = "Something"
                };
                var destination = Mapper.Map<Source, Desitination>(source);

                destination.GetProperty().ShouldBeNull();
            }
        }

        public class Source
        {
            public string Property { get; set; }
        }

        public class Desitination
        {
            string _property;

            public string Property
            {
                set { _property = value; }
            }

            public string GetProperty()
            {
                return _property;
            }
        }
    }
}