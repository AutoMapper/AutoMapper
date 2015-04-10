﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using AutoMapper.QueryableExtensions;
using Should;
using Xunit;

namespace AutoMapper.UnitTests.Bug
{
    public static class GenericTestExtensionMethods
    {
        public static bool Any<T>(this IEnumerable<T> self, Func<T,int,bool> func)
        {
            return self.Where(func).Any();
        }

        public static bool AnyParamReverse<T>(this IEnumerable<T> self, Func<T, T, bool> func)
        {
            return self.Any(t => func(t,t));
        }

        public static bool Lambda<T>(this T self, Func<T, bool> func)
        {
            return func(self);
        }
    }

    public class ExpressionMapping : NonValidatingSpecBase
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
            public ChildDTO GrandChild { get; set; }
            public int ID_ { get; set; }
            public int? IDs { get; set; }
            public int ID2 { get; set; }
        }

        public class Parent
        {
            public ICollection<Child> Children { get; set; }

            private Child _child;
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
            public int? ID2 { get; set; }
        }

        private Expression<Func<ParentDTO, bool>> _predicateExpression;
        private Parent _valid; 

        protected override void Establish_context()
        {
            Mapper.CreateMap<Parent, ParentDTO>().ReverseMap();
            Mapper.CreateMap<Child, ChildDTO>()
                .ForMember(d => d.ID_, opt => opt.MapFrom(s => s.ID))
                .ReverseMap()
                .ForMember(d => d.ID, opt => opt.MapFrom(s => s.ID_));
        }

        public override void MainTeardown()
        {
            Should_Validate();
            base.MainTeardown();
        }

        private void Should_Validate()
        {
            var expression = Mapper.Map<Expression<Func<Parent, bool>>>(_predicateExpression);
            var items = new[] {_valid}.AsQueryable();
            items.Where(expression).ShouldContain(_valid);
            var items2 = items.UseAsDataSource().For<ParentDTO>().Where(_predicateExpression);
            items2.Count().ShouldEqual(1);
        }

        [Fact]
        public void When_Use_Outside_Class_Method_Call()
        {
            var ids = new[] { 4, 5 };
            _predicateExpression = p => p.Children.Where((c, i) => c.ID_ > 4).Any(c => ids.Contains(c.ID_));
            _valid = new Parent { Children = new[] { new Child { ID = 5 } } };
        }

        [Fact]
        public void When_Use_Property_From_Child_Property()
        {
            _predicateExpression = p => p.Child.ID_ > 4;
            _valid = new Parent { Child= new Child { ID = 5 } };
        }

        [Fact]
        public void When_Use_Null_Substitution_Mappings_Against_Constants()
        {
            _predicateExpression = p => p.Child.ID_ > 4;
            _valid = new Parent { Child = new Child { ID = 5 } };
        }

        [Fact]
        public void When_Use_Null_Substitution_Mappings_Against_Constants_Reverse_Order()
        {
            _predicateExpression = p => 4 < p.Child.ID_;
            _valid = new Parent { Child = new Child { ID = 5 } };
        }

        [Fact]
        public void When_Use_Reverse_Null_Substitution_Mappings_Against_Constants()
        {
            _predicateExpression = p => p.Child.ID2 > 4;
            _valid = new Parent {Child = new Child {ID2 = 5}};
        }

        [Fact]
        public void When_Use_Reverse_Null_Substitution_Mappings_Against_Constants_Reverse_Order()
        {
            _predicateExpression = p => 4 < p.Child.ID2;
            _valid = new Parent { Child = new Child { ID2 = 5 } };
        }

        [Fact]
        public void When_Use_Sub_Lambda_Statement()
        {
            _predicateExpression = p => p.Children.Any(c => c.ID_ > 4);
            _valid = new Parent { Children = new[] { new Child { ID = 5 } } };
        }

        [Fact]
        public void When_Use_Multiple_Parameter_Lambda_Statement()
        {
            _predicateExpression = p => p.Children.Any((c, i) => c.ID_ > 4);
            _valid = new Parent { Children = new[] { new Child { ID = 5 } } };
        }

        [Fact]
        public void When_Use_Lambda_Statement_With_TypeMapped_Property_Being_Other_Than_First()
        {
            _predicateExpression = p => p.Children.AnyParamReverse((c, c2) => c.ID_ > 4);
            _valid = new Parent {Children = new[] {new Child {ID = 5}}};
        }

        [Fact]
        public void When_Use_Child_TypeMap_In_Sub_Lambda_Statement()
        {
            _predicateExpression = p => p.Children.Any(c => c.GrandChild.GrandChild.ID_ == 4);
            _valid = new Parent
            {
                Children = new[]
                {
                    new Child {GrandChild = new Child {GrandChild = new Child {ID = 4}}}
                }
            };
        }

        [Fact]
        public void When_Use_Parent_And_Child_Lambda_Parameters_In_Child_Lambda_Statement()
        {
            _predicateExpression = p => p.Children.Any(c => c.GrandChild.ID_ == p.Child.ID_);
            _valid = new Parent
            {
                Child = new Child {ID = 4},
                Children = new[] {new Child {GrandChild = new Child  {ID = 4}}}
            };
        }

        [Fact]
        public void When_Use_Lambdas_Where_Type_Matches_Parent_Object()
        {
            _predicateExpression = p => p.Child.Lambda(c => c.ID_ == 4);
            _valid = new Parent {Child = new Child {ID = 4}};
        }

        [Fact]
        public void When_Reusing_TypeMaps()
        {
            _predicateExpression = p => p.Child.Parent.Child.GrandChild.ID_ == 4;
            _valid = new Parent {Child = new Child {GrandChild = new Child {ID = 4}}};
        }

        [Fact]
        public void When_Using_Non_TypeMapped_Class_Property()
        {
            var year = DateTime.Now.Year;
            _predicateExpression = p => p.DateTime.Year == year;
            _valid = new Parent {DateTime = DateTime.Now};
        }

        [Fact]
        public void When_Using_Non_TypeMapped_Class_Method()
        {
            var year = DateTime.Now.Year;
            _predicateExpression = p => p.DateTime.Year.Equals(year);
            _valid = new Parent { DateTime = DateTime.Now };
        }

        [Fact]
        public void When_Using_Non_TypeMapped_Class_Property_Against_Constant()
        {
            _predicateExpression = p => p.DateTime.Year.ToString() == "2015";
            _valid = new Parent { DateTime = DateTime.Now };
        }

        [Fact]
        public void When_Using_Non_TypeMapped_Class_Method_Against_Constant()
        {
            _predicateExpression = p => p.DateTime.Year.ToString().Equals("2015");
            _valid = new Parent { DateTime = DateTime.Now };
        }

        [Fact]
        public void When_Using_Everything_At_Once()
        {
            var year = DateTime.Now.Year;
            _predicateExpression = p => p.DateTime.Year == year && p.Child.Parent.Child.GrandChild.Parent.Child.GrandChild.GrandChild.ID_ == 4 && p.Children.Any(c => c.GrandChild.GrandChild.ID_ == 4);
            _valid = new Parent { DateTime = DateTime.Now, Child = new Child { GrandChild = new Child { GrandChild = new Child { ID = 4 } } }, Children = new[] { new Child { GrandChild = new Child { GrandChild = new Child { ID = 4 } } } } };
        }
    }


    public class A<T> : IQueryable<T>
    {
        public IEnumerator<T> GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return GetEnumerator();
        }

        public Expression Expression { get; private set; }
        public Type ElementType { get; private set; }
        public IQueryProvider Provider { get; private set; }
    }
}