using System;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.EquivilencyExpression;
using Should;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ExpressionToExpressionMapping : IDisposable
    {
        public class Base
        {
            public Sub Sub { get; set; }
        }

        public class Sub
        {
            public int ID { get; set; }
        }

        public class BaseDTO
        {
            public int SubID { get; set; }
        }

        [Fact]
        public void Expression_to_Expression_should_work_with_flattening()
        {
            Mapper.CreateMap<Base, BaseDTO>();
            var expression = Mapper.Map<Expression<Func<BaseDTO, bool>>, Expression<Func<Base, bool>>>(dto => dto.SubID == 5);
            var items = new object[10].Select((o, i)=> new Base {Sub = new Sub {ID = i}}).ToList();
            items.AsQueryable().FirstOrDefault(expression).ShouldBeSameAs(items[5]);
        }

        [Fact]
        public void Object_to_Expression_should_work_with_flattening()
        {
            Mapper.CreateMap<BaseDTO, Base>().EqualityComparision((b, bdto) => b.SubID == bdto.Sub.ID);
            var expression = Mapper.Map<BaseDTO, Expression<Func<Base, bool>>>(new BaseDTO{SubID = 5});
            var items = new object[10].Select((o, i) => new Base { Sub = new Sub { ID = i } }).ToList();
            items.AsQueryable().FirstOrDefault(expression).ShouldBeSameAs(items[5]);
        }

        public void Dispose()
        {
            Mapper.Reset();
        }
    }
}