using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public class ExpressionMapping : AutoMapperSpecBase
    {
        public class ParentDTO
        {
            public ICollection<ChildDTO> Children { get; set; }
            public ChildDTO Child { get; set; }
            public DateTime DateTime { get; set; }
        }

        public class ChildDTO
        {
            public ParentDTO Parent { get; set; }
            public int ID2 { get; set; }
            public ChildDTO GrandChild { get; set; }
            public int IDs { get; set; }
        }

        public class Parent
        {
            private Child _child;
            public ICollection<Child> Children { get; set; }

            public Child Child
            {
                get { return _child; }
                set
                {
                    _child = value;
                    _child.Parent = this;
                }
            }
            public DateTime DateTime { get; set; }
        }

        public class Child
        {
            private Parent _parent;

            public Parent Parent
            {
                get { return _parent; }
                set
                {
                    _parent = value;
                    if (GrandChild != null)
                        GrandChild.Parent = _parent;
                }
            }

            public int ID { get; set; }
            public Child GrandChild { get; set; }
            public int IDs { get; set; }
        }

        [Fact]
        public void Expression_Mapping_Fails_When_Use_Sub_Enumerable_Mapping()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            var ids = new[] {4, 5};
            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Children.Where((c,i) => c.ID2 > 4).Any(c => ids.Contains(c.ID2));
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents = new[] { new Parent { Children = new[] { new Child { ID = 4 } } }, new Parent { Children = new[] { new Child { ID = 5 } } } }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void Expression_Mapping_Fails_When_Use_Sub_Mapping()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Child.ID2 > 4;
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents = new[] { new Parent { Child = new Child { ID = 4 } }, new Parent { Child= new Child { ID = 5 } } }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void Crazy_MultiTiered_Super_Mapping_Test()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Children.Any(c => c.GrandChild.GrandChild.IDs == 4);
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {Children = new[] {new Child {GrandChild = new Child {GrandChild = new Child {IDs = 4}}}}},
                    new Parent {Children = new[] {new Child {GrandChild = new Child {GrandChild = new Child {IDs = 5}}}}}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void Crazy_MultiTiered_Super_Mapping_CustomResolver_Test()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Child.Parent.Child.GrandChild.ID2 == 4;
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {Child = new Child {GrandChild = new Child {ID = 4}}},
                    new Parent {Child = new Child {GrandChild = new Child {ID = 5}}}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void Crazyier_MultiTiered_Super_Mapping_CustomResolver_Test()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Child.Parent.Child.GrandChild.GrandChild.IDs == 4;
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {Child = new Child {GrandChild = new Child {GrandChild = new Child {IDs = 4}}}},
                    new Parent {Child = new Child {GrandChild = new Child {GrandChild = new Child {IDs = 5}}}}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void Why_Why_Why_I_Must_Know_Why_You_Would_Go_This_Far_Up_And_Down_The_Property_Tree()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));

            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.Child.Parent.Child.GrandChild.Parent.Child.GrandChild.GrandChild.IDs == 4 && p.Children.Any(c => c.GrandChild.GrandChild.ID2 == 4);
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {Child = new Child {GrandChild = new Child {GrandChild = new Child {IDs = 4}}},Children = new[] {new Child {GrandChild = new Child {GrandChild = new Child {ID = 4}}}}},
                    new Parent {Child = new Child {GrandChild = new Child {GrandChild = new Child {IDs = 5}}},Children = new[] {new Child {GrandChild = new Child {GrandChild = new Child {ID = 5}}}}}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void DateTimeExample()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));
            var year = DateTime.Now.Year;
            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.DateTime.Year == year;
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {DateTime = DateTime.Now},
                    new Parent {DateTime = DateTime.Now.AddYears(1)}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }

        [Fact]
        public void EqualsDateTimeExample()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID2, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID2));
            var year = DateTime.Now.Year;
            Expression<Func<ParentDTO, bool>> dtoExpression = p => p.DateTime.Year.Equals(year);
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(dtoExpression);
            var parents =
                new[]
                {
                    new Parent {DateTime = DateTime.Now},
                    new Parent {DateTime = DateTime.Now.AddYears(1)}
                }.AsQueryable();

            parents.Where(expression).Count().ShouldEqual(1);
        }
    }

}