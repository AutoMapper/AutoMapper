using Xunit;
using Should;
using System;
using AutoMapper.Mappers;

namespace AutoMapper.UnitTests.Bug
{
    public class InitialTypes : AutoMapperSpecBase
    {
        MyObjectMapper _mapper = new MyObjectMapper();

        public class Base
        {
            public string Name { get; set; }
        }

        public class DerivedA : Base
        {
            public string Gender { get; set; }
        }

        public class DerivedB : Base
        {
            public int Age { get; set; }
        }

        public class MyObjectMapper : IObjectMapper
        {
            public ResolutionContext Context;

            public object Map(ResolutionContext context, IMappingEngineRunner mapper)
            {
                Context = context;
                return context.DestinationValue;
            }

            public bool IsMatch(ResolutionContext context)
            {
                return context.SourceType == typeof(Base) && context.DestinationType == typeof(Base);
            }
        }

        protected override void Establish_context()
        {
            Mapper.Initialize(cfg =>
            {
                MapperRegistry.Mappers.Insert(0, _mapper);
                cfg.CreateMap<Base, Base>();
            });
        }

        protected override void Because_of()
        {
            Mapper.Map<Base>(new DerivedA());
        }

        [Fact]
        public void Should_set_initial_types()
        {
            _mapper.Context.ShouldNotBeNull();
            _mapper.Context.InitialSourceType.ShouldEqual(typeof(DerivedA));
            _mapper.Context.InitialDestinationType.ShouldEqual(typeof(Base));
        }
    }
}