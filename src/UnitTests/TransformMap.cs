using System;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class TransformPropertyMap : AutoMapperSpecBase
    {
        DateTime _now = DateTime.Now;
        private Destination _destination;
        private Source _source;

        public class Source
        {
            public string String { get; set; }

            public DateTime Date { get; set; }

            public DateTime DateToString { get; set; }
        }

        public class Destination
        {
            public string String { get; set; }

            public DateTime Date { get; set; }

            public string DateToString { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMissingTypeMaps = true;
            cfg.TransformPropertyMap<string>(s => $"Transformed({s})");
            cfg.TransformPropertyMap<DateTime>(s => s.AddDays(1));
            cfg.CreateProfile("Profile", p => p.CreateMap<Source, Source>());
        });

        protected override void Because_of()
        {
            _source = new Source
            {
                Date = _now,
                DateToString = _now,
                String = "Test String"
            };
            _destination = Mapper.Map<Destination>(_source);
        }

        [Fact]
        public void Sould_TransformPropertyMap_Date_Using_DateTime_Transform()
        {
            _destination.Date.ShouldEqual(_now.AddDays(1));
        }

        [Fact]
        public void Sould_TransformPropertyMap_String_String_Transform()
        {
            _destination.String.ShouldEqual("Transformed(Test String)");
        }

        [Fact]
        public void Sould_TransformPropertyMap_DateToString_String_Transform_But_Not_Date_Transform()
        {
            _destination.DateToString.ShouldEqual($"Transformed({_now})");
        }

        [Fact]
        public void Sould_Not_TransformPropertyMap_In_Different_Profile()
        {
            var source = Mapper.Map<Source>(_source);
            source.ShouldNotEqual(_source);
            source.Date.ShouldEqual(_source.Date);
            source.String.ShouldEqual(_source.String);
            source.DateToString.ShouldEqual(_source.DateToString);
        }
    }
}