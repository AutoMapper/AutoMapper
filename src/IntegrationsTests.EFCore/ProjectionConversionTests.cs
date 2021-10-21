using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using System;
using System.Linq;
using Xunit;

namespace IntegrationsTests.EFCore
{
    public class ProjectionConversionTests
    {
        class Source
        {
            public int Id { get; set; }
            public SourceValue Value { get; set; }
        }

        class Destination
        {
            public DestinationValue Value { get; set; }
        }

        class NullableSource
        {
            public int Id { get; set; }
            public SourceValue? Value { get; set; }
        }

        class NullableDestination
        {
            public DestinationValue? Value { get; set; }
        }

        enum SourceValue
        {
            Id
        }

        struct DestinationValue
        {
            public string Value { get; }

            public DestinationValue(string value)
            {
                Value = value;
            }

            public static implicit operator DestinationValue(SourceValue source) => new DestinationValue(source.ToString());
        }

        private class ClientContext : DbContext
        {
            public DbSet<Source> Source1 { get; set; }
            public DbSet<NullableSource> Source2 { get; set; }

            protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
            {
                optionsBuilder.UseInMemoryDatabase(Guid.NewGuid().ToString());
            }

            protected override void OnModelCreating(ModelBuilder modelBuilder)
            {
                base.OnModelCreating(modelBuilder);
            }
        }

        private readonly IMapper _mapper = new MapperConfiguration(config =>
        {
            config.CreateMap<Source, Destination>();
            config.CreateMap<NullableSource, NullableDestination>();
            //config.CreateMap<SourceValue, DestinationValue>().ConvertUsing(src => src);
            //config.CreateMap<SourceValue?, DestinationValue?>().ConvertUsing(src => src);
        }).CreateMapper();

        [Fact]
        public void Should_use_implicit_operator()
        {
            using var context = new ClientContext();
            context.Source1.Add(new Source { Value = SourceValue.Id });
            context.SaveChanges();

            _mapper.ProjectTo<Destination>(context.Source1).Single().Value.Value.ShouldBe("Id");
        }

        [Fact]
        public void Should_use_implicit_operator_if_pair_is_nullable_and_is_null()
        {
            using var context = new ClientContext();
            context.Source2.Add(new NullableSource { Value = null });
            context.SaveChanges();

            _mapper.ProjectTo<NullableDestination>(context.Source2).Single().Value.ShouldBe(null);
        }

        [Fact]
        public void Should_use_implicit_operator_if_pair_is_nullable_and_has_value()
        {
            using var context = new ClientContext();
            context.Source2.Add(new NullableSource { Value = SourceValue.Id });
            context.SaveChanges();

            _mapper.ProjectTo<NullableDestination>(context.Source2).Single().Value.Value.Value.ShouldBe("Id");
        }
    }
}
