using System;
using System.Collections.Generic;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class PreserveReferencesParentChildCacheBug : AutoMapperSpecBase
    {
        public class ParentDTO
        {
            public List<ChildDTO> Children { get; set; } = new List<ChildDTO>();
            public int Id { get; set; }
        }

        public class ChildDTO
        {
            public int Id { get; set; }
            public ParentDTO Parent { get; set; }
        }

        public class ParentModel
        {
            public List<ChildModel> Children { get; set; } = new List<ChildModel>();
            public int Id { get; set; }
        }

        public class ChildModel
        {
            public ChildModel(ParentModel parent) 
            {
                Parent = parent;
            }

            public int Id { get; set; }
            public ParentModel Parent { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ParentDTO, ParentModel>().PreserveReferences();

            cfg.CreateMap<ChildDTO, ChildModel>()
                .ForMember(c => c.Parent, o => o.Ignore())
                .PreserveReferences();
        });

        [Fact]
        public void Map_ChildrenCollection_Should_Not_Throw_Key_Exception()
        {
            var parentDto = new ParentDTO { Id = 1 };
            for (var i = 0; i < 5; i++)
            {
                parentDto.Children.Add(new ChildDTO { Id = i, Parent = parentDto });
            }

            typeof(Exception).ShouldNotBeThrownBy(() => 
                Mapper.Map<List<ChildDTO>, List<ChildModel>>(parentDto.Children)); 
        }
    }
}
