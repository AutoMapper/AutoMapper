using Xunit;
using Should;
using System;
using System.Collections.Generic;

namespace AutoMapper.UnitTests.Bug
{
    public class UseDestinationValue : AutoMapperSpecBase
    {
        public class OrganizationDTO
        {
            public long? ID { get; set; }
            public string Name { get; set; }

            private CollectionDTOController<BranchDTO, short> _branchCollection;
            public CollectionDTOController<BranchDTO, short> BranchCollection
            {
                get
                {
                    if(_branchCollection == null)
                        _branchCollection = new CollectionDTOController<BranchDTO, short>();

                    return _branchCollection;
                }
                set { _branchCollection = value; }
            }
        }

        public class BranchDTO
        {
            public short? ID { get; set; }
            public string Name { get; set; }

        }

        public class CollectionDTOController<T, K>
           where T : class
           where K : struct
        {
            public IEnumerable<T> Models { get; set; }
            public K? SelectedID { get; set; }
        }

        public class Organization
        {
            public long? ID { get; set; }
            public string Name { get; set; }

            private CollectionController<Branch, short, EventArgs> _BranchCollection;
            public CollectionController<Branch, short, EventArgs> BranchCollection
            {
                get
                {
                    if(_BranchCollection == null)
                        _BranchCollection = new CollectionController<Branch, short, EventArgs>(this);

                    return _BranchCollection;
                }
                set { _BranchCollection = value; }
            }
        }

        public class Branch
        {
            public short? ID { get; set; }
            public string Name { get; set; }

        }

        public class CollectionController<T, K, Z>
            where T : class
            where K : struct
            where Z : EventArgs
        {
            private object _owner;
            public CollectionController(object owner)
            {
                _owner = owner;
            }
            public IEnumerable<T> Models { get; set; }
            public K? SelectedID { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrganizationDTO, Organization>().ForMember(d=>d.BranchCollection, o=>o.UseDestinationValue());
            cfg.CreateMap<BranchDTO, Branch>();
            cfg.CreateMap(typeof(CollectionDTOController<,>), typeof(CollectionController<,,>));
        });

        protected override void Because_of()
        {
            var branchDto = new BranchDTO { ID = 51, Name = "B1" };
            var orgDto = new OrganizationDTO { ID = 5, Name = "O1" };
            orgDto.BranchCollection.Models = new BranchDTO[] { branchDto };

            Mapper.Map<Organization>(orgDto);
        }
    }

    public class DontUseDestinationValue : NonValidatingSpecBase
    {
        public class OrganizationDTO
        {
            public long? ID { get; set; }
            public string Name { get; set; }

            private CollectionDTOController<BranchDTO, short> _branchCollection;
            public CollectionDTOController<BranchDTO, short> BranchCollection
            {
                get
                {
                    if(_branchCollection == null)
                        _branchCollection = new CollectionDTOController<BranchDTO, short>();

                    return _branchCollection;
                }
                set { _branchCollection = value; }
            }
        }

        public class BranchDTO
        {
            public short? ID { get; set; }
            public string Name { get; set; }

        }

        public class CollectionDTOController<T, K>
           where T : class
           where K : struct
        {
            public IEnumerable<T> Models { get; set; }
            public K? SelectedID { get; set; }
        }

        public class Organization
        {
            public long? ID { get; set; }
            public string Name { get; set; }

            private CollectionController<Branch, short, EventArgs> _BranchCollection;
            public CollectionController<Branch, short, EventArgs> BranchCollection
            {
                get
                {
                    if(_BranchCollection == null)
                        _BranchCollection = new CollectionController<Branch, short, EventArgs>(this);

                    return _BranchCollection;
                }
                set { _BranchCollection = value; }
            }
        }

        public class Branch
        {
            public short? ID { get; set; }
            public string Name { get; set; }

        }

        public class CollectionController<T, K, Z>
            where T : class
            where K : struct
            where Z : EventArgs
        {
            private object _owner;
            public CollectionController(object owner)
            {
                _owner = owner;
            }
            public IEnumerable<T> Models { get; set; }
            public K? SelectedID { get; set; }
        }

        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<OrganizationDTO, Organization>();
            cfg.CreateMap<BranchDTO, Branch>();
            cfg.CreateMap(typeof(CollectionDTOController<,>), typeof(CollectionController<,,>));
        });

        [Fact]
        public void Should_report_missing_constructor()
        {
            var branchDto = new BranchDTO { ID = 51, Name = "B1" };
            var orgDto = new OrganizationDTO { ID = 5, Name = "O1" };
            orgDto.BranchCollection.Models = new BranchDTO[] { branchDto };

            new Action(()=>Mapper.Map<Organization>(orgDto)).ShouldThrow<AutoMapperMappingException>(
                ex=>ex.InnerException.Message.ShouldStartWith(typeof(CollectionController<Branch, short, EventArgs>) + " needs to have a constructor with 0 args or only optional args"));
        }
    }
}