using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using Should;
using Should.Core.Assertions;
using Xunit;

namespace AutoMapper.UnitTests.Mappers.Dynamic
{
    class Destination
    {
        public string Foo { get; set; }
        public string Bar { get; set; }
        internal string Jack { get; set; }
        public int[] Data { get; set; }
    }

    public class DynamicDictionary : DynamicObject
    {
        private readonly Dictionary<string, object> dictionary = new Dictionary<string, object>();

        public override bool TryGetMember(GetMemberBinder binder, out object result)
        {
            return dictionary.TryGetValue(binder.Name, out result);
        }

        public override bool TrySetMember(SetMemberBinder binder, object value)
        {
            dictionary[binder.Name] = value;
            return true;
        }

        public int Count => dictionary.Count;
    }

    public class EntityA
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifiedBy { get; set; }
        public byte[] RowVersion { get; set; }
    }

    public class DomainA : DynamicDictionary
    {
        public string Name { get; set; }
        public string Description { get; set; }
    }

    public class EntityB
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string Description { get; set; }
        public DateTime CreationDate { get; set; }
        public string CreatedBy { get; set; }
        public DateTime ModifyDate { get; set; }
        public string ModifiedBy { get; set; }
        public byte[] RowVersion { get; set; }
        public List<EntityA> Items { get; set; }
    }

    public class DomainB : DynamicDictionary
    {

        public string Name { get; set; }
        public string Description { get; set; }
        public List<DomainA> Items { get; set; }
    }

    public class When_mapping_to_dynamic_from_getter_only_property
    {
        class Source
        {
            public Source()
            {
                Value = 24;
            }

            public int Value { get; }
        }

        [Fact]
        public void Should_map_source_properties()
        {
            var config = new MapperConfiguration(cfg => { });
            dynamic destination = config.CreateMapper().Map<DynamicDictionary>(new Source());
            ((int)destination.Count).ShouldEqual(1);
            Assert.Equal(24, destination.Value);
        }
    }

    public class When_mapping_to_dynamic
    {
        dynamic _destination;

        [Fact]
        public void Should_map_source_properties()
        {
            var config = new MapperConfiguration(cfg => { });
            var data = new[] { 1, 2, 3 };
            _destination = config.CreateMapper().Map<DynamicDictionary>(new Destination { Foo = "Foo", Bar = "Bar", Data = data });
            ((int)_destination.Count).ShouldEqual(3);
            Assert.Equal("Foo", _destination.Foo);
            Assert.Equal("Bar", _destination.Bar);
            ((int[])_destination.Data).SequenceEqual(data).ShouldBeTrue();
        }
    }

    public class When_mapping_from_dynamic
    {
        Destination _destination;

        [Fact]
        public void Should_map_destination_properties()
        {
            dynamic source = new DynamicDictionary();
            source.Foo = "Foo";
            source.Bar = "Bar";
            source.Jack = "Jack";
            var config = new MapperConfiguration(cfg => { });
            _destination = config.CreateMapper().Map<Destination>(source);
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldEqual("Bar");
            _destination.Jack.ShouldBeNull();
        }
    }

    public class When_mapping_struct_from_dynamic
    {
        Destination _destination;

        struct Destination
        {
            public string Foo { get; set; }
            public string Bar { get; set; }
            internal string Jack { get; set; }
        }

        [Fact]
        public void Should_map_destination_properties()
        {
            dynamic source = new DynamicDictionary();
            source.Foo = "Foo";
            source.Bar = "Bar";
            source.Jack = "Jack";
            var config = new MapperConfiguration(cfg => { });
            _destination = config.CreateMapper().Map<Destination>(source);
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldEqual("Bar");
            _destination.Jack.ShouldBeNull();
        }
    }

    public class When_mapping_from_dynamic_with_missing_property
    {
        Destination _destination;

        [Fact]
        public void Should_map_existing_properties()
        {
            dynamic source = new DynamicDictionary();
            source.Foo = "Foo";
            var config = new MapperConfiguration(cfg => { });
            _destination = config.CreateMapper().Map<Destination>(source);
            _destination.Foo.ShouldEqual("Foo");
            _destination.Bar.ShouldBeNull();
        }
    }

    public class When_mapping_from_dynamic_to_dynamic
    {
        dynamic _destination;

        [Fact]
        public void Should_map()
        {
            dynamic source = new DynamicDictionary();
            source.Foo = "Foo";
            source.Bar = "Bar";
            var config = new MapperConfiguration(cfg => { });
            _destination = config.CreateMapper().Map<DynamicDictionary>(source);
            Assert.Equal("Foo", _destination.Foo);
            Assert.Equal("Bar", _destination.Bar);
        }
    }

    public class When_mapping_from_partial_dynamic_to_class
    {
        [Fact]
        public void Should_map()
        {
            // Arrange
            var entity = new EntityA
            {
                Id = 100,
                Name = "ENTITY1",
                Description = "Entity 1",
                CreatedBy = "unittest",
                CreationDate = DateTime.UtcNow,
                ModifiedBy = "unittest",
                ModifyDate = DateTime.UtcNow,
                RowVersion = new byte[] {0x00, 0x00, 0x00, 0x00, 0xfa, 0x23, 0x45, 0xf3},
            };
            var config = new MapperConfiguration(cfg => { });
            var mapper = config.CreateMapper();

            // Act
            var domain = mapper.Map<DomainA>(entity);
            var dynamicDomain = (dynamic) domain;
            var newEntity = mapper.Map<EntityA>(domain);

            //Assert
            Assert.NotNull(domain);
            Assert.Equal(entity.Name, domain.Name);
            Assert.Equal(entity.Description, domain.Description);
            Assert.Equal(entity.Id, dynamicDomain.Id);
            Assert.Equal(entity.CreatedBy, dynamicDomain.CreatedBy);
            Assert.Equal(entity.CreationDate, dynamicDomain.CreationDate);
            Assert.Equal(entity.ModifiedBy, dynamicDomain.ModifiedBy);
            Assert.Equal(entity.ModifyDate, dynamicDomain.ModifyDate);
            Assert.Equal(entity.RowVersion, dynamicDomain.RowVersion);

            Assert.Equal(entity.Name, newEntity.Name);
            Assert.Equal(entity.Description, newEntity.Description);
            Assert.Equal(entity.Id, newEntity.Id);
            Assert.Equal(entity.CreatedBy, newEntity.CreatedBy);
            Assert.Equal(entity.CreationDate, newEntity.CreationDate);
            Assert.Equal(entity.ModifiedBy, newEntity.ModifiedBy);
            Assert.Equal(entity.ModifyDate, newEntity.ModifyDate);
            Assert.Equal(entity.RowVersion, newEntity.RowVersion);
        }
    }

    public class When_mapping_from_partial_dynamic_with_list_of_partial_dynamic_to_class
    {
        [Fact]
        public void Should_map()
        {
            // Arrange
            var entityA1 = new EntityA
            {
                Id = 100,
                Name = "ENTITYA1",
                Description = "Entity A1",
                CreatedBy = "unittest",
                CreationDate = DateTime.UtcNow,
                ModifiedBy = "unittest",
                ModifyDate = DateTime.UtcNow,
                RowVersion = new byte[] { 0x00, 0x00, 0x00, 0x00, 0xfc, 0x23, 0x45, 0xf3 },
            };

            var entityA2 = new EntityA
            {
                Id = 100,
                Name = "ENTITYA2",
                Description = "Entity A2",
                CreatedBy = "unittest",
                CreationDate = DateTime.UtcNow,
                ModifiedBy = "unittest",
                ModifyDate = DateTime.UtcNow,
                RowVersion = new byte[] { 0x00, 0x00, 0x00, 0x00, 0xfd, 0x23, 0x45, 0xf3 },

            };

            var entityB1 = new EntityB
            {
                Id = 100,
                Name = "ENTITYB1",
                Description = "Entity B1",
                CreatedBy = "unittest",
                CreationDate = DateTime.UtcNow,
                ModifiedBy = "unittest",
                ModifyDate = DateTime.UtcNow,
                RowVersion = new byte[] {0x00, 0x00, 0x00, 0x00, 0xfa, 0x23, 0x45, 0xf3},
                Items = new List<EntityA> {entityA1, entityA2}
            };
            var mapper = new MapperConfiguration(cfg => { }).CreateMapper();

            // Act
            var domainB1 = mapper.Map<DomainB>(entityB1);
            var dynamicDomainB1 = (dynamic)domainB1;
            var newEntityB1 = mapper.Map<EntityB>(domainB1);

            //Assert
            Assert.NotNull(domainB1);
            Assert.Equal(entityB1.Name, domainB1.Name);
            Assert.Equal(entityB1.Description, domainB1.Description);
            Assert.Equal(entityB1.Id, dynamicDomainB1.Id);
            Assert.Equal(entityB1.CreatedBy, dynamicDomainB1.CreatedBy);
            Assert.Equal(entityB1.CreationDate, dynamicDomainB1.CreationDate);
            Assert.Equal(entityB1.ModifiedBy, dynamicDomainB1.ModifiedBy);
            Assert.Equal(entityB1.ModifyDate, dynamicDomainB1.ModifyDate);
            Assert.Equal(entityB1.RowVersion, dynamicDomainB1.RowVersion);

            Assert.Equal(domainB1.Items.Count, 2);
            Assert.Equal(domainB1.Items[0].Count, 6);
            Assert.Equal(domainB1.Items[0].Name, entityA1.Name);
            Assert.Equal(domainB1.Items[0].Description, entityA1.Description);
            Assert.Equal(domainB1.Items[1].Count, 6);
            Assert.Equal(domainB1.Items[1].Name, entityA2.Name);
            Assert.Equal(domainB1.Items[1].Description, entityA2.Description);

            Assert.Equal(entityB1.Name, newEntityB1.Name);
            Assert.Equal(entityB1.Description, newEntityB1.Description);
            Assert.Equal(entityB1.Id, newEntityB1.Id);
            Assert.Equal(entityB1.CreatedBy, newEntityB1.CreatedBy);
            Assert.Equal(entityB1.CreationDate, newEntityB1.CreationDate);
            Assert.Equal(entityB1.ModifiedBy, newEntityB1.ModifiedBy);
            Assert.Equal(entityB1.ModifyDate, newEntityB1.ModifyDate);
            Assert.Equal(entityB1.RowVersion, newEntityB1.RowVersion);
            Assert.Equal(entityB1.Items.Count, newEntityB1.Items.Count);

            Assert.Equal(entityB1.Items[0].Id, newEntityB1.Items[0].Id);
            Assert.Equal(entityB1.Items[0].Name, newEntityB1.Items[0].Name);
            Assert.Equal(entityB1.Items[0].Description, newEntityB1.Items[0].Description);
            Assert.Equal(entityB1.Items[0].CreatedBy, newEntityB1.Items[0].CreatedBy);
            Assert.Equal(entityB1.Items[0].CreationDate, newEntityB1.Items[0].CreationDate);
            Assert.Equal(entityB1.Items[0].ModifiedBy, newEntityB1.Items[0].ModifiedBy);
            Assert.Equal(entityB1.Items[0].ModifyDate, newEntityB1.Items[0].ModifyDate);
            Assert.Equal(entityB1.Items[0].RowVersion, newEntityB1.Items[0].RowVersion);

            Assert.Equal(entityB1.Items[1].Id, newEntityB1.Items[1].Id);
            Assert.Equal(entityB1.Items[1].Name, newEntityB1.Items[1].Name);
            Assert.Equal(entityB1.Items[1].Description, newEntityB1.Items[1].Description);
            Assert.Equal(entityB1.Items[1].CreatedBy, newEntityB1.Items[1].CreatedBy);
            Assert.Equal(entityB1.Items[1].CreationDate, newEntityB1.Items[1].CreationDate);
            Assert.Equal(entityB1.Items[1].ModifiedBy, newEntityB1.Items[1].ModifiedBy);
            Assert.Equal(entityB1.Items[1].ModifyDate, newEntityB1.Items[1].ModifyDate);
            Assert.Equal(entityB1.Items[1].RowVersion, newEntityB1.Items[1].RowVersion);
        }
    }
}