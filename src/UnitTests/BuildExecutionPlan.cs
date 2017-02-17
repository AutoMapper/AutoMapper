using System;
using AutoMapper;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class BuildExecutionPlan : AutoMapperSpecBase
    {
        Model _source;
        Dto _destination;

        public class Model
        {
            public Guid? Id { get; set; }
            public Guid? FooId { get; set; }
            public string FullDescription { get; set; }
            public string ShortDescription { get; set; }
            public DateTime Date { get; set; }
            public int? IntValue { get; set; }
        }

        public class Dto
        {
            public Guid? Id { get; set; }
            public string FooId { get; set; }
            public string FullDescription { get; set; }
            public string ShortDescription { get; set; }
            public DateTime Date { get; set; }
            public int IntValue { get; set; }
            public string CompanyName { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(c =>
        {
            c.CreateMap<Model, Dto>().ForMember(d => d.CompanyName, o => o.Ignore());
        });

        protected override void Because_of()
        {
            _source = new Model
            {
                Id = Guid.NewGuid(),
                FooId = Guid.NewGuid(),
                ShortDescription = "Yoyodyne Foo",
                FullDescription = "Deluxe Foo Manufactured by Yoyodyne, Inc.",
                Date = DateTime.Now,
                IntValue = 13,
            };
            var plan = Configuration.BuildExecutionPlan(typeof(Model), typeof(Dto));
            var context = ((IRuntimeMapper)Mapper).DefaultContext;
            _destination = ((Func<Model, Dto, ResolutionContext, Dto>)plan.Compile())(_source, null, context);
        }

        [Fact]
        public void Should_build_the_execution_plan()
        {
            _destination.Id.ShouldEqual(_source.Id);
            _destination.FooId.ShouldEqual(_source.FooId.ToString());
            _destination.ShortDescription.ShouldEqual(_source.ShortDescription);
            _destination.FullDescription.ShouldEqual(_source.FullDescription);
            _destination.Date.ShouldEqual(_source.Date);
            _destination.IntValue.ShouldEqual(_source.IntValue.Value);
        }
    }
}