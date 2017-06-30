using System.Collections.Generic;
using AutoMapper;
using Shouldly;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
    namespace Dictionaries
    {
        [TestFixture]
        public class SimpleExample
        {
            public class SourceValue
            {
                public int Value { get; set; }
            }
            
            public class DestValue
            {
                public int Value { get; set; }
            }

            [Test]
            public void Example()
            {
                var config = new MapperConfiguration(cfg =>
                {
                    cfg.CreateMap<SourceValue, DestValue>();
                });

                var sourceDict = new Dictionary<string, SourceValue>
                {
                    {"First", new SourceValue {Value = 5}},
                    {"Second", new SourceValue {Value = 10}},
                    {"Third", new SourceValue {Value = 15}}
                };

                var destDict = config.CreateMapper().Map<Dictionary<string, SourceValue>, IDictionary<string, DestValue>>(sourceDict);

                destDict.Count.ShouldBe(3);
                destDict["First"].Value.ShouldBe(5);
                destDict["Second"].Value.ShouldBe(10);
                destDict["Third"].Value.ShouldBe(15);
            }
        }
    }
}