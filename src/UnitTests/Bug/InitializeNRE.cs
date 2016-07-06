using Xunit;
using Should;
using System;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class InitializeNRE : AutoMapperSpecBase
    {

        public class TestEntity
        {
            public string SomeData { get; set; }

            public int SomeCount { get; set; }

            public ICollection<string> Tags { get; set; }
        }

        public class TestViewModel
        {
            public string SomeData { get; set; }

            public int SomeCount { get; set; }

            public string Tags { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<TestEntity, TestViewModel>();
        });
    }
}