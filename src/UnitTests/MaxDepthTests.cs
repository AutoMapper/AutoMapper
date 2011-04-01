﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;

namespace AutoMapper.UnitTests
{
    [TestFixture]
    public class MaxDepthTests
    {
        public class Source
        {
            public int Level { get; set; }
            public IList<Source> Children { get; set; }
            public Source Parent { get; set; }

            public Source(int level)
            {
                Children = new List<Source>();
                Level = level;
            }

            public void AddChild(Source child)
            {
                Children.Add(child);
                child.Parent = this;
            }
        }

        public class Destination
        {
            public int Level { get; set; }
            public IList<Destination> Children { get; set; }
            public Destination Parent { get; set; }
        }

        private Source _source;

        [TestFixtureSetUp]
        public void Initializer()
        {
            var nest = new Source(1);

            nest.AddChild(new Source(2));
            nest.Children[0].AddChild(new Source(3));
            nest.Children[0].AddChild(new Source(3));
            nest.Children[0].Children[1].AddChild(new Source(4));
            nest.Children[0].Children[1].AddChild(new Source(4));
            nest.Children[0].Children[1].AddChild(new Source(4));

            nest.AddChild(new Source(2));
            nest.Children[1].AddChild(new Source(3));

            nest.AddChild(new Source(2));
            nest.Children[2].AddChild(new Source(3));

            _source = nest;
        }

        [SetUp]
        public void BeforeTest()
        {
            Mapper.Reset();
        }

        [Test]
        public void Second_level_children_are_null_with_max_depth_1()
        {
            Mapper.CreateMap<Source, Destination>().MaxDepth(1);
            var destination = Mapper.Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                Assert.IsNull(child);
            }
        }

        [Test]
        public void Second_level_children_are_not_null_with_max_depth_2()
        {
            Mapper.CreateMap<Source, Destination>().MaxDepth(2);
            var destination = Mapper.Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                Assert.AreEqual(2, child.Level);
                Assert.IsNotNull(child);
                Assert.AreEqual(destination, child.Parent);
            }
        }

        [Test]
        public void Third_level_children_are_null_with_max_depth_2()
        {
            Mapper.CreateMap<Source, Destination>().MaxDepth(2);
            var destination = Mapper.Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                Assert.IsNotNull(child.Children);
                foreach (var subChild in child.Children)
                {
                    Assert.IsNull(subChild);
                }
            }
        }

        [Test]
        public void Third_level_children_are_not_null_max_depth_3()
        {
            Mapper.CreateMap<Source, Destination>().MaxDepth(3);
            var destination = Mapper.Map<Source, Destination>(_source);
            foreach (var child in destination.Children)
            {
                Assert.IsNotNull(child.Children);
                foreach (var subChild in child.Children)
                {
                    Assert.AreEqual(3, subChild.Level);
                    Assert.IsNotNull(subChild.Children);
                    Assert.AreEqual(child, subChild.Parent);
                }
            }
        }
    }
}
