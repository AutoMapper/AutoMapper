using System.Collections.Generic;
using System.Linq;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public interface IBaseQueryableInterface
    {
        IPropertyInterface PropertyInterface { get; set; }
    }

    public interface IQueryableInterface : IBaseQueryableInterface
    {
        string AnotherProperty { get; set; }
    }

    public interface IPropertyInterface
    {
        int PropertyInterfaceId { get; set; }
    }

    public class QueryableInterfaceImpl : IQueryableInterface
    {
        public IPropertyInterface PropertyInterface { get; set; }
        public string AnotherProperty { get; set; }
    }

    public class PropertyInterfaceImpl : IPropertyInterface
    {
        public int PropertyInterfaceId { get; set; }
    }

    public class QueryableDto
    {
        public int PropertyInterfaceId { get; set; }
        public string AnotherProperty { get; set; }
    }


    public class QueryableInterfaceInheritanceIssue : AutoMapperSpecBase
    {
        [Fact]
        public void QueryableShouldMapSpecifiedBaseInterfaceMember()
        {
            var inputList =
                new List<IQueryableInterface>()
                {
                    new QueryableInterfaceImpl()
                    {
                        AnotherProperty = "One",
                        PropertyInterface = new PropertyInterfaceImpl() {PropertyInterfaceId = 1}
                    },
                    new QueryableInterfaceImpl()
                    {
                        AnotherProperty = "Two",
                        PropertyInterface = new PropertyInterfaceImpl() {PropertyInterfaceId = 2}
                    }
                };
            // Currently throws exception
            var result = inputList.AsQueryable().ProjectTo<QueryableDto>(ConfigProvider).ToList();
            result.ShouldNotBeNull();
            result.ShouldBeOfLength(2);
            result.FirstOrDefault(dto => dto.AnotherProperty == "One" && dto.PropertyInterfaceId == 1).ShouldNotBeNull();
            result.FirstOrDefault(dto => dto.AnotherProperty == "Two" && dto.PropertyInterfaceId == 2).ShouldNotBeNull();
        }


        protected override MapperConfiguration Configuration
        {
            get
            {
                return
                    new MapperConfiguration(
                        cfg =>
                            cfg.CreateMap<IQueryableInterface, QueryableDto>()
                                .ForMember(dto => dto.PropertyInterfaceId,
                                    opt => opt.MapFrom(qi => qi.PropertyInterface.PropertyInterfaceId)));
            }
        }
    }
}