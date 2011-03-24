using NUnit.Framework;
using NUnit.Framework.SyntaxHelpers;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class SettersInBaseClasses
    {
        [TestFixtureSetUp]
        public void SetUp(){
            Mapper.CreateMap<Source, GrandGrandChild>();
            Mapper.CreateMap<Source, GrandChild>();
            Mapper.CreateMap<Source, Child>();

            Mapper.CreateMap<Source, GrandGrandChildPrivate>();
            Mapper.CreateMap<Source, GrandChildPrivate>();
            Mapper.CreateMap<Source, ChildPrivate>();
        }

        [Test]
        public void PublicSetterInParentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, Child>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
        }

        
        [Test]
        public void PublicSetterInGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandChild>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
        }

        [Test]
        public void PublicSetterInGrandGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandGrandChild>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
        }

        [Test]
        public void PrivateSetterInParentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, ChildPrivate>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
        }

        [Test]
        public void PrivateSetterInGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandChildPrivate>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
        }

        [Test]
        public void PrivateSetterInGrandGrandparentWorks()
        {
            var source = new Source {ParentProperty = "ParentProperty", ChildProperty = 1};
            var target = Mapper.Map<Source, GrandGrandChildPrivate>(source);
            Assert.That(target.ParentProperty, Is.EqualTo(source.ParentProperty) );
            Assert.That(target.ChildProperty, Is.EqualTo(source.ChildProperty) );
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