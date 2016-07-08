using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class SamePropertyNameOnParentAndChildClasses : AutoMapperSpecBase
    {
        protected override MapperConfiguration Configuration { get; } = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<BU, BUVM>(); 
            cfg.CreateMap<BUVM, BU>()
                .ForMember(d => d.CostCenters, opt => opt.Ignore());
            cfg.CreateMap<CostCenter, CostCenterVM>().MaxDepth(2);
            cfg.CreateMap<CostCenterVM, CostCenter>().MaxDepth(2);
        });

        [Fact]
        public void should_not_throw_exception_on_expression_map()
        {
            Expression<Func<CostCenterVM, bool>> _predicate = x => x.Name == "aa" && x.Id == 10 && x.AA == "bb" && x.BUId == 1 && x.BU.Id == 1 && x.BU.Name == "bb";
            var newPredicate = Mapper.Map<Expression<Func<CostCenter, bool>>>(_predicate);
        }

        public abstract class BaseVM
        {
            public int Id { get; set; }
        }

        public class CostCenterVM : BaseVM
        {
            public string Name { get; set; }
            public string AA { get; set; }
            public int BUId { get; set; }
            public virtual BUVM BU { get; set; }
        }


        public class BUVM : BaseVM
        {
            public string Name { get; set; }
            public string Code { get; set; }
        }

        public abstract class AuditableEntity<T>
        {
            public virtual T Id { get; set; }
        }

        public class CostCenter : AuditableEntity<int>
        {
            public string Name { get; set; }
            public string AA { get; set; }
            public int BUId { get; set; }
            public virtual BU BU { get; set; }
        }

        public class BU : AuditableEntity<int>
        {
            public string Name { get; set; }
            public string Code { get; set; }
            public virtual ICollection<CostCenter> CostCenters { get; set; }
        }

    }
}