using AutoMapper.Features;
using Shouldly;
using System.Collections.Generic;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class ConfigurationFeatureTest
    {
        [Fact]
        public void Adding_same_feature_multiple_times_should_replace_eachother()
        {
            var featureA = new ConfigurationExpressionFeatureA(1);
            var featureB = new ConfigurationExpressionFeatureB(1);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddOrUpdateFeature(new ConfigurationExpressionFeatureA(3));
                cfg.AddOrUpdateFeature(new ConfigurationExpressionFeatureA(2));
                cfg.AddOrUpdateFeature(featureA);
                cfg.AddOrUpdateFeature(new ConfigurationExpressionFeatureB(3));
                cfg.AddOrUpdateFeature(new ConfigurationExpressionFeatureB(2));
                cfg.AddOrUpdateFeature(featureB);
            });

            Validate<ConfigurationFeatureA>(featureA, config);
            Validate<ConfigurationFeatureB>(featureB, config);
        }

        [Fact]
        public void Add_single_feature()
        {
            var featureA = new ConfigurationExpressionFeatureA(1);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddOrUpdateFeature(featureA);
            });

            Validate<ConfigurationFeatureA>(featureA, config);
        }

        [Fact]
        public void Add_multiple_features()
        {
            var featureA = new ConfigurationExpressionFeatureA(1);
            var featureB = new ConfigurationExpressionFeatureB(2);
            var config = new MapperConfiguration(cfg =>
            {
                cfg.AddOrUpdateFeature(featureA);
                cfg.AddOrUpdateFeature(featureB);
            });

            Validate<ConfigurationFeatureA>(featureA, config);
            Validate<ConfigurationFeatureB>(featureB, config);
        }

        private void Validate<TFeature>(ConfigurationExpressionFeatureBase feature, MapperConfiguration config)
            where TFeature : ConfigurationFeatureBase
        {
            feature.ConfigurationProviders.ShouldBeOfLength(1);

            var configurationFeature = config.Features.Get<TFeature>();
            configurationFeature.ShouldNotBeNull();
            configurationFeature.Value.ShouldBe(feature.Value);
            configurationFeature.SealedCount.ShouldBe(1);
        }

        public class ConfigurationExpressionFeatureA : ConfigurationExpressionFeatureBase<ConfigurationFeatureA>
        {
            public ConfigurationExpressionFeatureA(int value) : base(value, new ConfigurationFeatureA(value))
            {
            }
        }

        public class ConfigurationExpressionFeatureB : ConfigurationExpressionFeatureBase<ConfigurationFeatureB>
        {
            public ConfigurationExpressionFeatureB(int value) : base(value, new ConfigurationFeatureB(value))
            {
            }
        }

        public abstract class ConfigurationExpressionFeatureBase<TFeature> : ConfigurationExpressionFeatureBase
            where TFeature : IFeature
        {
            private readonly TFeature _feature;

            protected ConfigurationExpressionFeatureBase(int value, TFeature feature)
                : base(value)
            {
                _feature = feature;
            }

            public override void Configure(IConfigurationProvider configurationProvider)
            {
                ConfigurationProviders.Add(configurationProvider);
                configurationProvider.Features.AddOrUpdate(_feature);
            }
        }

        public abstract class ConfigurationExpressionFeatureBase : IGlobalFeature
        {
            public int Value { get; }
            public List<IConfigurationProvider> ConfigurationProviders { get; } = new List<IConfigurationProvider>();

            protected ConfigurationExpressionFeatureBase(int value)
            {
                Value = value;
            }

            public abstract void Configure(IConfigurationProvider configurationProvider);
        }

        public class ConfigurationFeatureA : ConfigurationFeatureBase
        {
            public ConfigurationFeatureA(int value) : base(value)
            {
            }
        }

        public class ConfigurationFeatureB : ConfigurationFeatureBase
        {
            public ConfigurationFeatureB(int value) : base(value)
            {
            }
        }

        public abstract class ConfigurationFeatureBase : IFeature
        {
            public int SealedCount { get; private set; }
            public int Value { get; }

            public ConfigurationFeatureBase(int value)
            {
                Value = value;
            }

            void IFeature.Seal(IConfigurationProvider configurationProvider)
            {
                SealedCount++;
            }
        }
    }
}