using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Projection
{
    using NameSourceType = String        ; using NameDtoType = String       ; // Example of Reference Type
    using DescSourceType = Nullable<int> ; using DescDtoType = Nullable<int>; // Example of Value Type

    public class MembersToExpand_ExpressionArray  : AutoMapperSpecBase
    {
        public class Source
        {   
            public NameSourceType Name { get; set; }
            public DescSourceType Desc { get; set; }
        }
        public class Dto
        {
            public NameDtoType Name { get; set; }
            public DescDtoType Desc { get; set; }
        }
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dto>();
        });

        private static readonly IQueryable<Source> _iq = new List<Source> {
            new Source() { Name = "Name1", Desc = -12, },
        } .AsQueryable();

        private static readonly Source _iqf = _iq.First();

        [Fact] public void ProjectReferenceType() => _iq.ProjectTo<Dto>(Configuration, _ => _.Name             ).First().Name.ShouldEqual(_iqf.Name);
        [Fact] public void ProjectValueType    () => _iq.ProjectTo<Dto>(Configuration,              _ => _.Desc).First().Desc.ShouldEqual(_iqf.Desc);
        [Fact] public void ProjectBoth         () => _iq.ProjectTo<Dto>(Configuration, _ => _.Name, _ => _.Desc);
    }
}
