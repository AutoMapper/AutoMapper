using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    using NameSourceType = String        ; using NameDtoType = String         ; // Example of Reference Type
    using DescSourceType = Int32         ; using DescDtoType = Nullable<Int32>; // Example of Value Type mapped to appropriate Nullable

    public class MembersToExpand_ExpressionArray  : AutoMapperSpecBase
    {
        public class SourceDeepInner { public DescSourceType Desc { get; set; } }
        public class SourceInner     { public DescSourceType Desc { get; set; } public SourceDeepInner Deep { get; set; } }
        public class Source
        {   
            public NameSourceType Name { get; set; }
            public DescSourceType Desc { get; set; }
            public SourceInner Inner { get; set; }
        }
        public class Dto
        {
            public NameDtoType Name { get; set; }
            public DescDtoType Desc { get; set; }
            public DescDtoType InnerDescFlattened { get; set; }
            public DescDtoType      DeepFlattened { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dto>()
                .ForMember(dto => dto.InnerDescFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner     .Desc); })
                .ForMember(dto => dto.     DeepFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Deep.Desc); })
        ;
        });

        private static readonly IQueryable<Source> _iq = new List<Source> {
            new Source() { Name = "Name1", Desc = -12, Inner = new SourceInner {Desc = -25, Deep = new SourceDeepInner() { Desc = 28 } } },
        } .AsQueryable();

        private static readonly Source _iqf = _iq.First();

        [Fact] public void ProjectReferenceType() => _iq.ProjectTo<Dto>(Configuration, _ => _.Name             ).First().Name.ShouldEqual(_iqf.Name);
        [Fact] public void ProjectValueType    () => _iq.ProjectTo<Dto>(Configuration,              _ => _.Desc).First().Desc.ShouldEqual(_iqf.Desc);
        [Fact] public void ProjectBoth         () => _iq.ProjectTo<Dto>(Configuration, _ => _.Name, _ => _.Desc);

        [Fact] public void ProjectInner()     => _iq.ProjectTo<Dto>(Configuration, _ => _.InnerDescFlattened).First().InnerDescFlattened.ShouldEqual(_iqf.Inner     .Desc);
        [Fact] public void ProjectDeepInner() => _iq.ProjectTo<Dto>(Configuration, _ => _.     DeepFlattened).First().     DeepFlattened.ShouldEqual(_iqf.Inner.Deep.Desc);
    }
}
