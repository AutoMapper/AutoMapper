using System;

namespace AutoMapper.UnitTests.MappingInheritance
{
    public class IncludeBaseShouldNotCreateMaps : AutoMapperSpecBase
    {
        public abstract class BaseBaseSource { }
        public class BaseSource : BaseBaseSource
        {
            public string Foo { get; set; }
        }
        public class Source : BaseSource { }

        public abstract class BaseBaseDest
        {
            public string Foo { get; set; }
        }
        public class BaseDest : BaseBaseDest { }
        public class Dest : BaseDest { }

        public class TestProfile : Profile
        {
            protected override void Configure()
            {
                CreateMap<BaseSource, BaseDest>();
                CreateMap<Source, Dest>()
                    .IncludeBase<BaseSource, BaseDest>();
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg => cfg.AddProfile<TestProfile>());
    }
}