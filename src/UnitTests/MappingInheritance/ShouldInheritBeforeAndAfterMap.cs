﻿using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.MappingInheritance
{
    public class ShouldInheritBeforeAndAfterMap
    {
        public class BaseClass
        {
            public string Prop { get; set; }
        } 
        public class Class : BaseClass {}

        public class BaseDto
        {
            public string DifferentProp { get; set; }            
        }
        public class Dto : BaseDto {}

        [Fact]
        public void should_inherit_base_beforemap()
        {
            // arrange
            var source = new Class{ Prop = "test" };
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<BaseClass, BaseDto>()
                    .BeforeMap((s, d) => d.DifferentProp = s.Prop)
                    .Include<Class, Dto>();

                cfg.CreateMap<Class, Dto>();
            });
            var mappingEngine = configurationProvider.CreateMapper();

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }

        [Fact]
        public void should_inherit_base_aftermap()
        {
            // arrange
            var source = new Class { Prop = "test" };
            var configurationProvider = new MapperConfiguration(cfg =>
            {
                cfg
                    .CreateMap<BaseClass, BaseDto>()
                    .AfterMap((s, d) => d.DifferentProp = s.Prop)
                    .Include<Class, Dto>();

                cfg.CreateMap<Class, Dto>();
            });
            var mappingEngine = configurationProvider.CreateMapper();

            // act
            var dest = mappingEngine.Map<Class, Dto>(source);

            // assert
            "test".ShouldEqual(dest.DifferentProp);
        }
    }
}