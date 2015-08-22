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
            public void Projection_expression_can_be_used_for_conversion()
            {
                string friendlyGreeting = "Hello there!";

                Mapper.Initialize(x => {
                    x.CreateMap<Source, Target>()
                        .ProjectAndConvertUsing(s => new Target { String = friendlyGreeting });
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