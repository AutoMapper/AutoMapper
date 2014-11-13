using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Xunit;
using Should;
using AutoMapper.Impl;

namespace AutoMapper.UnitTests.Bug
{
    public class ForMemberAndForSourceMemberGenericsBug : SpecBase
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

        interface ISourceModelWithExtraProperties
        {
            string A { get; }
            string B { get; }
            string C { get; }
        }

        class SourceWithExtraProperties : ISourceModelWithExtraProperties
        {
            public int Id { get; set; }
            public string A { get; set; }
            public string B { get; set; }
            public string C { get; set; }
        }

        [Fact]
        public void ForMember_maps_all_properties_when_destination_properties_are_from_abstract_class()
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
        public void ForMember_maps_all_properties_when_destination_properties_are_from_interface()
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

        [Fact]
        public void ForSourceMember_maps_all_properties_when_destination_properties_are_from_interface()
        {
            Mapper.Initialize(mapper =>
            {
                var expression = mapper.CreateMap<SourceWithExtraProperties, DestinationModelFromInterface>()
                    .ForMember(dst => dst.DstId, cfg => cfg.MapFrom(src => src.Id))
                    .ForMember(dst => dst.DstFirstName, cfg => cfg.UseValue("John"))
                    .ForMember(dst => dst.DstLastName, cfg => cfg.UseValue("Doe"));
                ExtensionForSourceMemberInterface(expression);
            });
            var source = new SourceWithExtraProperties() { Id = 12345, A = "a", B = "b", C = "c" };

            var typeMap = Mapper.FindTypeMapFor(typeof(SourceWithExtraProperties), typeof(DestinationModelFromInterface));
            var aConfig = typeMap.FindOrCreateSourceMemberConfigFor(typeof(SourceWithExtraProperties).GetProperty("A"));
            aConfig.IsIgnored().ShouldBeTrue();
            var bConfig = typeMap.FindOrCreateSourceMemberConfigFor(typeof(SourceWithExtraProperties).GetProperty("B"));
            bConfig.IsIgnored().ShouldBeTrue();
            var cConfig = typeMap.FindOrCreateSourceMemberConfigFor(typeof(SourceWithExtraProperties).GetProperty("C"));
            cConfig.IsIgnored().ShouldBeTrue();
        }

        private static IMappingExpression<TSource, TDestination> ExtensionForSourceMemberInterface<TSource, TDestination>(IMappingExpression<TSource, TDestination> expression)
            where TSource : ISourceModelWithExtraProperties
        {
            return expression.ForSourceMember(src => src.A, cfg => cfg.Ignore())
                             .ForSourceMember(src => src.B, cfg => cfg.Ignore())
                             .ForSourceMember(src => src.C, cfg => cfg.Ignore());
        }
    }
}
