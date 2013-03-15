﻿using Xunit;
using Should;

namespace AutoMapper.UnitTests.Bug
{
    public class SettersInBaseClasses
    {
        public SettersInBaseClasses()
        {
            SetUp();
        }

        public void SetUp(){
            Mapper.CreateMap<Source, GrandGrandChild>();
            Mapper.CreateMap<Source, GrandChild>();
            Mapper.CreateMap<Source, Child>();

            Mapper.CreateMap<Source, GrandGrandChildPrivate>();
            Mapper.CreateMap<Source, GrandChildPrivate>();
            Mapper.CreateMap<Source, ChildPrivate>();
        }

        [Fact]
        public void PublicSetterInParentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, Child>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }

        
        [Fact]
        public void PublicSetterInGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandChild>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }

        [Fact]
        public void PublicSetterInGrandGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandGrandChild>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }

#if SILVERLIGHT
        [Fact(Skip = "Not supported in Silverlight 4")]
#else
        [Fact]
#endif
        public void PrivateSetterInParentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, ChildPrivate>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }

#if SILVERLIGHT
        [Fact(Skip = "Not supported in Silverlight 4")]
#else
        [Fact]
#endif
        public void PrivateSetterInGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandChildPrivate>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }

#if SILVERLIGHT
        [Fact(Skip = "Not supported in Silverlight 4")]
#else
        [Fact]
#endif
        public void PrivateSetterInGrandGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandGrandChildPrivate>(source);
            target.ParentProperty.ShouldEqual(source.ParentProperty);
            target.ChildProperty.ShouldEqual(source.ChildProperty);
        }
    }

    public class Source
    {
        public string ParentProperty { get; set; }
        public int ChildProperty{get; set;}
    }

    public class Parent {
        public string ParentProperty{get; set;}
    }

    public class Child : Parent {
        public int ChildProperty {get; set;}
    }

    public class GrandChild : Child {
    }

    public class GrandGrandChild : GrandChild {
    }

    public class ParentPrivate {
        public string ParentProperty{get; private set;}
    }

    public class ChildPrivate : ParentPrivate {
        public int ChildProperty {get;private set;}
    }

    public class GrandChildPrivate : ChildPrivate {
    }

    public class GrandGrandChildPrivate : GrandChildPrivate {
    }
}