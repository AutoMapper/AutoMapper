using System;

namespace AutoMapper.UnitTests.Projection
{
    namespace NullableItemsTests
    {
        using System.Linq;
        using QueryableExtensions;
        using Should;
        using Should.Core.Assertions;
        using Xunit;

        
        public class ProjectionDeclaration
        {          
            [Fact]
            public void Projection_expression_should_be_used_for_mapping()
            {
                string friendlyGreeting = "Hello there!";

                Mapper.Initialize(x => {
                    x.CreateMap<Source, Target>()
                        .ProjectUsing(s => new Target { String = friendlyGreeting });
                });
                
                var target = Mapper.Map<Target>(new Source());

                target.String.ShouldEqual(friendlyGreeting);
            }
            

            class Source { }
            
            class Target
            {
                public string String { get; set; }
            }
            
        }
    }
}