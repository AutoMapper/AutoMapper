namespace AutoMapper.UnitTests.Bug
{
    namespace DuplicateValuesBug
    {
        public class SourceObject
        {
            public int Id;
            public IList<SourceObject> Children;

            public void AddChild(SourceObject childObject)
            {
                if (this.Children == null)
                    this.Children = new List<SourceObject>();

                Children.Add(childObject);
            }
        }


        public class DestObject
        {
            public int Id;
            public IList<DestObject> Children;

            public DestObject()
            {
            }

            public void AddChild(DestObject childObject)
            {
                if (this.Children == null)
                    this.Children = new List<DestObject>();

                Children.Add(childObject);
            }
        }
        public class DuplicateValuesIssue
        {
            [Fact]
            public void Should_map_the_existing_array_elements_over()
            {
                var sourceList = new List<SourceObject>();
                var destList = new List<DestObject>();

                var config = new MapperConfiguration(cfg => cfg.CreateMap<SourceObject, DestObject>().PreserveReferences());
                config.AssertConfigurationIsValid();

                var source1 = new SourceObject
                {
                    Id = 1,
                };
                sourceList.Add(source1);

                var source2 = new SourceObject
                {
                    Id = 2,
                };
                sourceList.Add(source2);

                source1.AddChild(source2); // This causes the problem

                config.CreateMapper().Map(sourceList, destList);

                destList.Count.ShouldBe(2);
                destList[0].Children.Count.ShouldBe(1);
                destList[0].Children[0].ShouldBeSameAs(destList[1]);
            }
        }
    }
}