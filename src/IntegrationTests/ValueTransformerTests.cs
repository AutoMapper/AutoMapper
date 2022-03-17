using System.ComponentModel.DataAnnotations;
using System.Threading.Tasks;
using AutoMapper.UnitTests;
using Microsoft.EntityFrameworkCore;
using Shouldly;
using Xunit;

namespace AutoMapper.IntegrationTests
{
    namespace ValueTransformerTests
    {
        public class BasicTransforming : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
            });

            [Fact]
            public async Task Should_transform_value()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class StackingTransformers : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
            });

            [Fact]
            public async Task Should_stack_transformers_in_order()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class DifferentProfiles : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.CreateProjection<Source, Dest>();
                cfg.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                cfg.CreateProfile("Other", p => p.ValueTransformers.Add<string>(dest => dest + "! No joke!"));
            });

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            [Fact]
            public async Task Should_not_apply_other_transform()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class StackingRootConfigAndProfileTransform : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>();
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyProfileFirstThenRoot()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy is straight up dope! No joke!");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class TransformingValueTypes : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public int Value { get; set; }
            }

            public class Dest
            {
                public int Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = 5 });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<int>(dest => dest * 2);
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>();
                    p.ValueTransformers.Add<int>(dest => dest + 3);
                });
            });

            [Fact]
            public async Task ShouldApplyProfileFirstThenRoot()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe((5 + 3) * 2);
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class StackingRootAndProfileAndMemberConfig : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }

            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>()
                        .ValueTransformers.Add<string>(dest => dest + ", for real,");
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyTypeMapThenProfileThenRoot()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy, for real, is straight up dope! No joke!");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }

        public class StackingTypeMapAndRootAndProfileAndMemberConfig : AutoMapperSpecBase, IAsyncLifetime
        {
            public class Source
            {
                [Key]
                public int Id { get; set; }
                public string Value { get; set; }
            }

            public class Dest
            {
                public string Value { get; set; }
            }

            public class Context : LocalDbContext
            {
                public DbSet<Source> Sources { get; set; }
            }

            public class DatabaseInitializer : CreateDatabaseIfNotExists<Context>
            {
                protected override void Seed(Context context)
                {
                    context.Sources.Add(new Source { Value = "Jimmy" });

                    base.Seed(context);
                }
            }
            protected override MapperConfiguration CreateConfiguration() => new(cfg =>
            {
                cfg.ValueTransformers.Add<string>(dest => dest + "! No joke!");
                cfg.CreateProfile("Other", p =>
                {
                    p.CreateProjection<Source, Dest>()
                        .AddTransform<string>(dest => dest + ", for real,")
                        .ForMember(d => d.Value, opt => opt.AddTransform(d => d + ", seriously"));
                    p.ValueTransformers.Add<string>(dest => dest + " is straight up dope");
                });
            });

            [Fact]
            public async Task ShouldApplyTypeMapThenProfileThenRoot()
            {
                using (var context = new Context())
                {
                    var dest = await ProjectTo<Dest>(context.Sources).SingleAsync();

                    dest.Value.ShouldBe("Jimmy, seriously, for real, is straight up dope! No joke!");
                }
            }

            public async Task InitializeAsync()
            {
                var initializer = new DatabaseInitializer();

                await initializer.Migrate();
            }

            public Task DisposeAsync() => Task.CompletedTask;
        }


    }
}