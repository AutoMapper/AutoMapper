using System;
using Should;
using AutoMapper;
using Xunit;

namespace AutoMapper.UnitTests.MappingInheritance
{
    public class ApplyIncludeBaseRecursively : AutoMapperSpecBase
    {
        ViewModel _destination;

        public class BaseEntity
        {
            public string Property1 { get; set; }
        }
        public class SubBaseEntity : BaseEntity { }

        public class SpecificEntity : SubBaseEntity
        {
            public bool Map { get; set; }
        }

        public class ViewModel
        {
            public string Property2 { get; set; }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BaseEntity, ViewModel>()
                .ForMember(vm => vm.Property2, opt => opt.ResolveUsing(e => e.Property1));

            cfg.CreateMap<SubBaseEntity, ViewModel>()
                .IncludeBase<BaseEntity, ViewModel>();

            cfg.CreateMap<SpecificEntity, ViewModel>()
                .IncludeBase<SubBaseEntity, ViewModel>()
                .ForMember(vm => vm.Property2, opt => opt.Condition(e => e.Map));
        });

        protected override void Because_of()
        {
            _destination = Mapper.Map<ViewModel>(new SpecificEntity{ Map = true, Property1 = "Test" });
        }

        [Fact]
        public void Should_apply_all_included_base_maps()
        {
            _destination.Property2.ShouldEqual("Test");
        }
    }
}