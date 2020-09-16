using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity;
using System.Linq;
using System.Text.RegularExpressions;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests.ProjectionWithExplicitExpansionExtension
{
    public static class Ext
    {
        public static void SqlShouldSelectColumn   (this string sqlSelect, string columnName)=> sqlSelect.ShouldContain($".[{columnName}] AS [{columnName}]");
        public static void SqlShouldNotSelectColumn(this string sqlSelect, string columnName)=> sqlSelect.ShouldNotContain(columnName);
        public static void SqlFromShouldStartWith  (this string sqlSelect, string tableName)
        {
            Regex regex = new Regex($@"FROM(\s+)\[dbo\]\.\[{tableName}\](\s+)AS");
            regex.Match(sqlSelect).Success.ShouldBeTrue();
            // sqlSelect.ShouldContain($"FROM [dbo].[{tableName}] AS");
        }
    }
}
namespace AutoMapper.IntegrationTests
{
    using UnitTests;
    using QueryableExtensions;
    using ProjectionWithExplicitExpansionExtension;

    using NameSourceType = String; using NameDtoType = String         ; // Example of Reference Type
    using DescSourceType = Int32 ; using DescDtoType = Nullable<Int32>; // Example of Value Type mapped to appropriate Nullable

    public class ProjectionWithExplicitExpansion : AutoMapperSpecBase
    {
        public class SourceDeepInner
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public DescSourceType Dide { get; set; }
            public DescSourceType Did1 { get; set; }
        }
        public class SourceInner
        {
            [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public DescSourceType Ides { get; set; }
            public DescSourceType Ide1 { get; set; }
            public SourceDeepInner Deep { get; set; }
        }
        public class Source
        {   [Key, DatabaseGenerated(DatabaseGeneratedOption.None)]
            public DescSourceType Desc { get; set; }
            public NameSourceType Name { get; set; }
            public SourceInner Inner { get; set; }
        }
        public class Dto
        {
            public NameDtoType Name { get; set; }
            public DescDtoType Desc { get; set; }
            public DescDtoType InnerDescFlattened   { get; set; }
            public DescDtoType InnerFlattenedNonKey { get; set; }
            public DescDtoType  DeepFlattened       { get; set; }
        }

        public class Context : DbContext
        {
            public List<string> Log = new List<string>();
            public Context()
            {
                Database.SetInitializer<Context>(new DatabaseInitializer());
                Database.Log += s => Log.Add(s);
            }

            public DbSet<Source> Sources { get; set; }
            public DbSet<SourceInner> SourceInners { get; set; }
            public DbSet<SourceDeepInner> SourceDeepInners { get; set; }

            public string GetLastSelectSqlLogEntry() => Log.Last(_ => _.TrimStart().StartsWith("SELECT"));
        }

        private static readonly IQueryable<Source> _iq = new List<Source> {
            new Source() { Name = "Name1", Desc = -12, Inner = new SourceInner {
                Ides = -25, Ide1 = -7,
                Deep = new SourceDeepInner() { Dide = 28, Did1 = 38,} } },
        } .AsQueryable();

        private static readonly Source _iqf = _iq.First();

        public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
        {
            protected override void Seed(Context context)
            {
                context.Sources.Add(_iqf);
                base.Seed(context);
            }
        }

        protected override MapperConfiguration Configuration => new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<Source, Dto>()
                .ForMember(dto => dto.Desc, conf => conf.ExplicitExpansion())
                .ForMember(dto => dto.Name, conf => conf.ExplicitExpansion())
                .ForMember(dto => dto.InnerDescFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Ides); })
                .ForMember(dto => dto.InnerFlattenedNonKey, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Ide1); })
                .ForMember(dto => dto.DeepFlattened, conf => { conf.ExplicitExpansion(); conf.MapFrom(_ => _.Inner.Deep.Dide); })
        ;});

        [Fact]
        public void NoExplicitExpansion()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources).ToList().First();
                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldNotContain("JOIN");
                sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
            }
        }

        [Fact]
        public void ProjectReferenceType()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Name).First();
                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldNotContain("JOIN");
                sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

                dto.Name.ShouldBe(_iqf.Name); sqlSelect.SqlShouldSelectColumn   (nameof(_iqf.Name)); 
                dto.Desc.ShouldBeNull()        ; sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));  
            }
        }
        [Fact]
        public void ProjectValueType()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Desc).First();

                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldNotContain("JOIN");
                sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

                dto.Desc.ShouldBe(_iqf.Desc); sqlSelect.ShouldContain   (nameof(_iqf.Desc));
                dto.Name.ShouldBeNull()        ; sqlSelect.ShouldNotContain(nameof(_iqf.Name)); 

            }
        }
        [Fact]
        public void ProjectBoth()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.Name, _ => _.Desc).First();

                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldNotContain("JOIN");
                sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); dto.InnerDescFlattened.ShouldBeNull();

                sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBe(_iqf.Name);
                sqlSelect.SqlShouldSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBe(_iqf.Desc);
            }
        }

        [Fact]
        public void ProjectInner()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.InnerDescFlattened).ToList().First();

                dto.InnerDescFlattened.ShouldBe(_iqf.Inner.Ides);
                dto.InnerFlattenedNonKey.ShouldBeNull();
                dto.DeepFlattened.ShouldBeNull();

                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldNotContain("JOIN"); // ???
                sqlSelect.ShouldNotContain(nameof(ctx.SourceInners)); // ???
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
            }
        }

        [Fact]
        public void ProjectInnerNonKey()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.InnerFlattenedNonKey).ToList().First();

                dto.InnerFlattenedNonKey.ShouldBe(_iqf.Inner.Ide1);
                dto.InnerDescFlattened.ShouldBeNull();
                dto.DeepFlattened.ShouldBeNull();

                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));
                sqlSelect.ShouldContain("JOIN");
                sqlSelect.ShouldContain(nameof(ctx.SourceInners));
                sqlSelect.ShouldNotContain(nameof(ctx.SourceDeepInners));
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
            }
        }

        [Fact]
        public void ProjectDeepInner()
        {
            using (var ctx = new Context())
            {
                var dto = ProjectTo<Dto>(ctx.Sources, null,  _ => _.DeepFlattened).ToList().First();
                var sqlSelect = ctx.GetLastSelectSqlLogEntry();
                sqlSelect.SqlFromShouldStartWith(nameof(ctx.Sources));

                dto.DeepFlattened.ShouldBe(_iqf.Inner.Deep.Dide);
                dto.InnerDescFlattened.ShouldBeNull();

                sqlSelect.ShouldContain("JOIN");
                sqlSelect.ShouldContain(nameof(ctx.SourceInners));
                sqlSelect.ShouldContain("JOIN"); // ???
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Name));   dto.Name.ShouldBeNull();
                sqlSelect.SqlShouldNotSelectColumn(nameof(_iqf.Desc));   dto.Desc.ShouldBeNull();
            }
        }
    }
}
