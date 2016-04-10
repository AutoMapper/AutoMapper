using System;
using System.Collections.Generic;
using System.Linq;
using AutoMapper;
using AutoMapper.Mappers;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
	public class NonGenericAllMembersNullSubstituteBug : SpecBase
	{
        public class Source
        {
            public int? Value1 { get; set; }
            public int? Value2 { get; set; }
            public int? Value3 { get; set; }
        }

        public class Destination
        {
            public string Value1 { get; set; }
            public string Value2 { get; set; }
            public string Value3 { get; set; }
        }

		[Fact]
		public void Should_map_all_null_values_to_its_substitute()
		{
            var config = new MapperConfiguration(cfg => cfg.CreateMap(typeof(Source), typeof(Destination))
                .ForAllMembers(opt => opt.NullSubstitute(string.Empty)));

		    var src = new Source
		    {
		        Value1 = 5
		    };

		    var mapper = config.CreateMapper();
		    var dest = mapper.Map<Source, Destination>(src);

		    dest.Value1.ShouldEqual("5");
		    dest.Value2.ShouldEqual(string.Empty);
		    dest.Value3.ShouldEqual(string.Empty);
		}
	}
}
