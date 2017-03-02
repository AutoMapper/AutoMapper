using Xunit;
using Should;
using AutoMapper.Mappers;
using System.Collections.Generic;
using System;
using System.Linq;

namespace AutoMapper.UnitTests.Bug
{
    public class MultidimensionalArrays : AutoMapperSpecBase
    {
        const int SomeValue = 154;
        Source _e = new Source(SomeValue);
        Destination[,] _destination;
        Source[,] _source;

        public class Source
        {
            public Source(int value)
            {
                Value = value;
            }
            public int Value { get; set; }
        }

        public class Destination
        {
            public int Value { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Destination>();
        });

        protected override void Because_of()
        {
            _source = new[,] { { _e, _e, new Source(2) },  { _e, new Source(11), _e }, { new Source(20), _e, _e }, {_e, _e, _e } };
            _destination = Mapper.Map<Destination[,]>(_source);
        }

        [Fact]
        public void Should_map_multidimensional_array()
        {
            _destination.GetLength(0).ShouldEqual(_source.GetLength(0));
            _destination.GetLength(1).ShouldEqual(_source.GetLength(1));
            _destination[0, 0].Value.ShouldEqual(SomeValue);
            _destination[0, 2].Value.ShouldEqual(2);
            _destination[1, 1].Value.ShouldEqual(11);
            _destination[2, 0].Value.ShouldEqual(20);
            _destination[3, 2].Value.ShouldEqual(SomeValue);
        }
    }

    public class FillMultidimensionalArray : SpecBase
    {
        int[,] _source;
        MultidimensionalArrayMapper.MultidimensionalArrayFiller _filler;
        protected override void Establish_context()
        {
            _source = new int[4,3];
            _filler = new MultidimensionalArrayMapper.MultidimensionalArrayFiller(_source);
        }

        protected override void Because_of()
        {
            for(int index = 0; index < _source.Length; index++)
            {
                _filler.NewValue(index);
            }
        }

        [Fact]
        public void Should_set_values_in_array()
        {
            int index = 0;
            foreach(var value in _source)
            {
                value.ShouldEqual(index);
                index++;
            }
            index.ShouldEqual(_source.Length);
        }
    }
}