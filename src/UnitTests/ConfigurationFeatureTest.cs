using AutoMapper.Features;

namespace AutoMapper.UnitTests;

public class ConfigurationFeatureTest
{
    [Fact]
    public void Adding_same_feature_multiple_times_should_replace_eachother()
    {
        var featureA = new ConfigurationExpressionFeatureA(1);
        var featureB = new ConfigurationExpressionFeatureB(1);
        var config = new MapperConfiguration(cfg =>
        {
            cfg.SetFeature(new ConfigurationExpressionFeatureA(3));
            cfg.SetFeature(new ConfigurationExpressionFeatureA(2));
            cfg.SetFeature(featureA);
            cfg.SetFeature(new ConfigurationExpressionFeatureB(3));
            cfg.SetFeature(new ConfigurationExpressionFeatureB(2));
            cfg.SetFeature(featureB);
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
            cfg.SetFeature(featureA);
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
            cfg.SetFeature(featureA);
            cfg.SetFeature(featureB);
        });

        Validate<ConfigurationFeatureA>(featureA, config);
        Validate<ConfigurationFeatureB>(featureB, config);
    }

    private void Validate<TFeature>(ConfigurationExpressionFeatureBase feature, MapperConfiguration config)
        where TFeature : ConfigurationFeatureBase
    {
        feature.ConfigurationProviders.ShouldBeOfLength(1);

        var configurationFeature = config.Internal().Features.Get<TFeature>();
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
        where TFeature : IRuntimeFeature
    {
        private readonly TFeature _feature;

        protected ConfigurationExpressionFeatureBase(int value, TFeature feature)
            : base(value)
        {
            _feature = feature;
        }

        public override void Configure(IGlobalConfiguration configurationProvider)
        {
            ConfigurationProviders.Add(configurationProvider);
            configurationProvider.Features.Set(_feature);
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

        public abstract void Configure(IGlobalConfiguration configurationProvider);
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

    public abstract class ConfigurationFeatureBase : IRuntimeFeature
    {
        public int SealedCount { get; private set; }
        public int Value { get; }

        public ConfigurationFeatureBase(int value)
        {
            Value = value;
        }

        void IRuntimeFeature.Seal(IGlobalConfiguration configurationProvider)
        {
            SealedCount++;
        }
    }
}