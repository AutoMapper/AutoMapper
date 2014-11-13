using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    public class ForMemberWrappedInAGenericExtensionMethodBug : SpecBase
    {
        abstract class DestinationModelBase
        {
            // Must have different property names so that auto-mapping doesn't
            // occur.
            public virtual int DstId { get; set; }
            public virtual string DstFirstName { get; set; }
            public string DstLastName = null;
        }

        interface IDestinationModel
        {
            int DstId { get; }
            string DstFirstName { get; }
            string DstLastName { get; }
        }

        class SourceModel
        {
            public int Id { get; set; }
            public string FirstName { get; set; }
            public string LastName;
        }

        class DestinationModelFromAbstract : DestinationModelBase
        {
            
        }

        class DestinationModelFromInterface : IDestinationModel
        {
            public int DstId { get; set; }
            public string DstFirstName { get; set; }
            public string DstLastName { get; set; }
        }

        [Fact]
        public void maps_all_properties_when_destination_properties_are_from_abstract_class()
        {
            Mapper.Initialize(mapper =>
            {
                var expression = mapper.CreateMap<SourceModel, DestinationModelFromAbstract>();
                ExtensionForMemberAbstract(expression);
            });
            var source = new SourceModel() { Id = 12345, FirstName = "John", LastName = "Doe" };

            var destination = Mapper.Map<DestinationModelFromAbstract>(source);

            destination.DstId.ShouldEqual(12345);
            destination.DstFirstName.ShouldEqual("John");
            destination.DstLastName.ShouldEqual("Doe");
        }

        private static IMappingExpression<SourceModel, TDestination> ExtensionForMemberAbstract<TDestination>(IMappingExpression<SourceModel, TDestination> expression)
            where TDestination : DestinationModelBase
        {
            return expression.ForMember(dst => dst.DstId, cfg => cfg.MapFrom(src => src.Id))
                             .ForMember(dst => dst.DstFirstName, cfg => cfg.MapFrom(src => src.FirstName))
                             .ForMember(dst => dst.DstLastName, cfg => cfg.MapFrom(src => src.LastName));
        }

        [Fact]
        public void maps_all_properties_when_destination_properties_are_from_interface()
        {
            Mapper.Initialize(mapper =>
            {
                var expression = mapper.CreateMap<SourceModel, DestinationModelFromInterface>();
                ExtensionForMemberInterface(expression);
            });
            var source = new SourceModel() { Id = 12345, FirstName = "John", LastName = "Doe" };

            var destination = Mapper.Map<DestinationModelFromInterface>(source);

            destination.DstId.ShouldEqual(12345);
            destination.DstFirstName.ShouldEqual("John");
            destination.DstLastName.ShouldEqual("Doe");
        }

        private static IMappingExpression<SourceModel, TDestination> ExtensionForMemberInterface<TDestination>(IMappingExpression<SourceModel, TDestination> expression)
            where TDestination : IDestinationModel
        {
            return expression.ForMember(dst => dst.DstId, cfg => cfg.MapFrom(src => src.Id))
                             .ForMember(dst => dst.DstFirstName, cfg => cfg.MapFrom(src => src.FirstName))
                             .ForMember(dst => dst.DstLastName, cfg => cfg.MapFrom(src => src.LastName));
        }
    }
}
