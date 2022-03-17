﻿namespace AutoMapper.UnitTests.Bug
{
    using Xunit;

    public class CaseSensitivityBug : AutoMapperSpecBase
    {
        protected override MapperConfiguration CreateConfiguration() => new(cfg =>
        {
            cfg.CreateMap<Foo, Bar>();
        });
        public class Foo
        {
            public int ID { get; set; }
        }

        public class Bar
        {
            public int id { get; set; }
        }
    }
}