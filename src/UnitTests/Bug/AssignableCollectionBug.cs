namespace AutoMapper.UnitTests.Bug
{
    namespace ByteArrayBug
    {
        public class When_mapping_byte_arrays : AutoMapperSpecBase
        {
            private Picture _source;
            private PictureDto _dest;

            public class Picture
            {
                public int Id { get; set; }
                public string Description { get; set; }
                public byte[] ImageData { get; set; }
            }

            public class PictureDto
            {
                public string Description { get; set; }
                public byte[] ImageData { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateMap<Picture, PictureDto>();
            });

            protected override void Because_of()
            {
                _source = new Picture {ImageData = new byte[100_000]};
                _dest = Mapper.Map<Picture, PictureDto>(_source);
            }

            [Fact]
            public void Should_copy_array()
            {
                _dest.ImageData.ShouldBe(_source.ImageData);
            }
        }
    }

    namespace AssignableLists
    {
        public class AutoMapperTests
        {
            [Fact]
            public void ListShouldNotMapAsReference()
            {
                // arrange

                var config = new MapperConfiguration(cfg => cfg.CreateMap<A, B>());
                var source = new A { Images = new List<string>() };

                // act
                var destination = config.CreateMapper().Map<B>(source);
                destination.Images.Add("test");

                // assert
                destination.Images.Count.ShouldBe(1);
                source.Images.Count.ShouldBe(0); // in 3.1.0 source.Images.Count is 1
            }
        }

        public class A
        {
            public IList<string> Images { get; set; }
        }

        public class B
        {
            public IList<string> Images { get; set; }
        }
    }
}
