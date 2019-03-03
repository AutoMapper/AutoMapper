﻿using Shouldly;
using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class MappingExpressionFeatureWithReverseTest
    {
        [Fact]
        public void Adding_same_feature_multiple_times_should_replace_eachother()
        {
            var featureA = new MappingExpressionFeatureA(1);
            var featureB = new MappingExpressionFeatureB(1);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .AddFeature(new MappingExpressionFeatureA(3))
                    .AddFeature(new MappingExpressionFeatureA(2))
                    .AddFeature(featureA)
                    .AddFeature(new MappingExpressionFeatureB(3))
                    .AddFeature(new MappingExpressionFeatureB(2))
                    .AddFeature(featureB)
                    .ReverseMap();
            });


            var typeMap = config.FindTypeMapFor<Source, Dest>();
            typeMap.Features.Count().ShouldBe(2);

            var typeMapReverse = config.FindTypeMapFor<Dest, Source>();
            typeMapReverse.Features.Count().ShouldBe(2);

            Validate(featureA);
            Validate(featureB);

            void Validate(MappingExpressionFeatureBase feature)
            {
                feature.ConfigureTypeMaps.ShouldBeOfLength(1);
                feature.ReverseMaps.ShouldBeOfLength(1);
            }
        }

        [Fact]
        public void Add_single_feature_with_reverse()
        {
            var featureA = new MappingExpressionFeatureA(1);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .AddFeature(featureA)
                    .ReverseMap();
            });

            var typeMap = config.FindTypeMapFor<Source, Dest>();
            typeMap.Features.Count().ShouldBe(1);

            var typeMapReverse = config.FindTypeMapFor<Dest, Source>();
            typeMapReverse.Features.Count().ShouldBe(1);

            Validate<TypeMapFeatureA>(featureA);

            void Validate<TFeature>(MappingExpressionFeatureBase feature)
                where TFeature : TypeMapFeatureBase
            {
                feature.ConfigureTypeMaps.ShouldBeOfLength(1);
                feature.ReverseMaps.ShouldBeOfLength(1);

                var typeMapFeature = typeMap.Features.Get<TFeature>();
                typeMapFeature.ShouldNotBeNull();
                typeMapFeature.Value.ShouldBe(feature.Value);
                typeMapFeature.SealedCount.ShouldBe(1);

                var typeMapFeatureReverse = typeMapReverse.Features.Get<TFeature>();
                typeMapFeatureReverse.ShouldNotBeNull();
                typeMapFeatureReverse.Value.ShouldBe(feature.Value + 1);
                typeMapFeatureReverse.SealedCount.ShouldBe(1);
            }
        }

        [Fact]
        public void Add_multiple_features_with_reverse()
        {
            var featureA = new MappingExpressionFeatureA(1);
            var featureB = new MappingExpressionFeatureB(2);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .AddFeature(featureA)
                    .AddFeature(featureB)
                    .ReverseMap();
            });

            var typeMap = config.FindTypeMapFor<Source, Dest>();
            typeMap.Features.Count().ShouldBe(2);

            var typeMapReverse = config.FindTypeMapFor<Dest, Source>();
            typeMapReverse.Features.Count().ShouldBe(2);

            Validate<TypeMapFeatureA>(featureA);
            Validate<TypeMapFeatureB>(featureB);

            void Validate<TFeature>(MappingExpressionFeatureBase feature)
                where TFeature : TypeMapFeatureBase
            {
                feature.ConfigureTypeMaps.ShouldBeOfLength(1);
                feature.ReverseMaps.ShouldBeOfLength(1);

                var typeMapFeature = typeMap.Features.Get<TFeature>();
                typeMapFeature.ShouldNotBeNull();
                typeMapFeature.Value.ShouldBe(feature.Value);
                typeMapFeature.SealedCount.ShouldBe(1);

                var typeMapFeatureReverse = typeMapReverse.Features.Get<TFeature>();
                typeMapFeatureReverse.ShouldNotBeNull();
                typeMapFeatureReverse.Value.ShouldBe(feature.Value + 1);
                typeMapFeatureReverse.SealedCount.ShouldBe(1);
            }
        }

        [Fact]
        public void Add_multiple_features_with_reverse_overriden()
        {
            var featureA = new MappingExpressionFeatureA(1);
            var featureB = new MappingExpressionFeatureB(2);
            var overridenFeatureB = new MappingExpressionFeatureB(10);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.CreateMap<Source, Dest>()
                    .AddFeature(featureA)
                    .AddFeature(featureB)
                    .ReverseMap()
                    .AddFeature(overridenFeatureB);
            });

            var typeMap = config.FindTypeMapFor<Source, Dest>();
            typeMap.Features.Count().ShouldBe(2);

            var typeMapReverse = config.FindTypeMapFor<Dest, Source>();
            typeMapReverse.Features.Count().ShouldBe(2);

            Validate<TypeMapFeatureA>(featureA, typeMap);
            Validate<TypeMapFeatureB>(featureB, typeMap);
            Validate<TypeMapFeatureA>(featureA, typeMapReverse, value: featureA.Value + 1);
            Validate<TypeMapFeatureB>(overridenFeatureB, typeMapReverse, 0);

            void Validate<TFeature>(MappingExpressionFeatureBase feature, TypeMap map, int reverseExecutedCount = 1, int? value = null)
                where TFeature : TypeMapFeatureBase
            {
                feature.ConfigureTypeMaps.ShouldBeOfLength(1);
                feature.ReverseMaps.ShouldBeOfLength(reverseExecutedCount);

                var typeMapFeature = map.Features.Get<TFeature>();
                typeMapFeature.ShouldNotBeNull();
                typeMapFeature.Value.ShouldBe(value ?? feature.Value);
                typeMapFeature.SealedCount.ShouldBe(1);
            }
        }

        public class MappingExpressionFeatureA : MappingExpressionFeatureBase<TypeMapFeatureA>
        {
            public MappingExpressionFeatureA(int value) : base(value, new TypeMapFeatureA(value), () => new MappingExpressionFeatureA(value + 1))
            {
            }
        }

        public class MappingExpressionFeatureB : MappingExpressionFeatureBase<TypeMapFeatureB>
        {
            public MappingExpressionFeatureB(int value) : base(value, new TypeMapFeatureB(value), () => new MappingExpressionFeatureB(value + 1))
            {
            }
        }

        public abstract class MappingExpressionFeatureBase<TFeature> : MappingExpressionFeatureBase
            where TFeature : IFeature
        {
            private readonly TFeature _feature;

            protected MappingExpressionFeatureBase(int value, TFeature feature, Func<IMappingExpressionFeature> reverseMappingExpressionFeature)
                : base(value, reverseMappingExpressionFeature)
            {
                _feature = feature;
            }

            public override void Configure(TypeMap typeMap)
            {
                ConfigureTypeMaps.Add(typeMap);
                typeMap.Features.Add(_feature);
            }
        }

        public abstract class MappingExpressionFeatureBase : IMappingExpressionFeature
        {
            public int Value { get; }
            public List<TypeMap> ConfigureTypeMaps { get; } = new List<TypeMap>();
            public List<IMappingExpressionFeature> ReverseMaps { get; } = new List<IMappingExpressionFeature>();

            private readonly Func<IMappingExpressionFeature> _reverseMappingExpressionFeature;

            protected MappingExpressionFeatureBase(int value, Func<IMappingExpressionFeature> reverseMappingExpressionFeature)
            {
                Value = value;
                _reverseMappingExpressionFeature = reverseMappingExpressionFeature;
            }

            public abstract void Configure(TypeMap typeMap);

            public IMappingExpressionFeature Reverse()
            {
                var reverse = _reverseMappingExpressionFeature();
                ReverseMaps.Add(reverse);
                return reverse;
            }
        }

        public class TypeMapFeatureA : TypeMapFeatureBase
        {
            public TypeMapFeatureA(int value) : base(value)
            {
            }
        }

        public class TypeMapFeatureB : TypeMapFeatureBase
        {
            public TypeMapFeatureB(int value) : base(value)
            {
            }
        }

        public abstract class TypeMapFeatureBase : IFeature
        {
            public int SealedCount { get; private set; }
            public int Value { get; }

            public TypeMapFeatureBase(int value)
            {
                Value = value;
            }

            void IFeature.Seal(IConfigurationProvider configurationProvider)
            {
                SealedCount++;
            }
        }


        public class Source
        {
            public int Value { get; set; }
        }

        public class Dest
        {
            public int Value { get; set; }
        }
    }
}
