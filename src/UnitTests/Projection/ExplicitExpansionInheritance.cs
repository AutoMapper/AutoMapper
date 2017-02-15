using System;
using System.Collections.Generic;
using System.Linq;

using AutoMapper.QueryableExtensions;

using Should;
using Xunit;

namespace AutoMapper.UnitTests.Projection 
{
    using NameSourceType = Nullable<int>; using NameDtoType = Nullable<int>;
    using DescSourceType = String; using DescDtoType = String;

    public class ExplicitExpansionInheritance  : AutoMapperSpecBase
    {
        private class Source
        {   
            public NameSourceType Name { get; set; }
            public DescSourceType Desc { get; set; }
        }
        public class DtoBase              { public NameDtoType Name { get; set; } }
        public class DtoDerived : DtoBase { public DescDtoType Desc { get; set; } }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, DtoBase>()
                .ForMember(dto => dto.Name, conf => { conf.MapFrom(src => src.Name); conf.ExplicitExpansion(); }).Include<Source, DtoDerived>();
            cfg.CreateMap<Source, DtoDerived>()
                .ForMember(dto => dto.Desc, conf => { conf.MapFrom(src => src.Desc); conf.ExplicitExpansion(); });
        });

        private static readonly IQueryable<Source> _iq = new List<Source> {
            new Source { Name = -25, Desc = "Descr", },
        } .AsQueryable();
        private static readonly Source _iqf = _iq.First();

        [Fact] public void ProjectAll() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(Configuration, _ => _.Name, _ => _.Desc);
            projectTo.Count().ShouldEqual(1); var first = projectTo.First();
            first.Desc.ShouldNotBeNull("Should be expanded.").ShouldEqual(_iqf.Desc);
            first.Name.ShouldHaveValue("Should be expanded.").ShouldEqual(_iqf.Name);
        }
        [Fact] public void BaseOnly() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(Configuration, _ => _.Name);
            projectTo.Count().ShouldEqual(1); var first = projectTo.First();
            first.Desc.ShouldBeNull("Should NOT be expanded.");
            first.Name.ShouldHaveValue("Should be expanded.").ShouldEqual(_iqf.Name);

        }
        [Fact] public void DerivedOnly() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(Configuration, _ => _.Desc);
            projectTo.Count().ShouldEqual(1); var first = projectTo.First();
            first.Desc.ShouldNotBeNull("Should be expanded.").ShouldEqual(_iqf.Desc);
            first.Name.ShouldBeNull("Should NOT be expanded.");
        }
        [Fact] public void SkipAll() 
        {
            var projectTo = _iq.ProjectTo<DtoDerived>(Configuration);
            projectTo.Count().ShouldEqual(1); var first = projectTo.First();
            first.Desc.ShouldBeNull("Should NOT be expanded.");
            first.Name.ShouldBeNull("Should NOT be expanded.");
        }
    }
}
