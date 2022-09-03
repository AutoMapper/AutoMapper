using System.Dynamic;

namespace AutoMapper.UnitTests.Bug;

public class MapOverloadsWithDynamic : AutoMapperSpecBase
{
    Settings _settings;

    class SubSetting
    {
        public int SubTimeout { get; set; }
        public string SubColour { get; set; }
    }

    class Settings
    {
        public int Timeout { get; set; }
        public string Colour { get; set; }
        public SubSetting SubSettings { get; set; }
    }

    protected override MapperConfiguration CreateConfiguration() => new(cfg => {});

    protected override void Because_of()
    {
        // The SubSettings property is another ExpandoObject.
        dynamic baseSettings = new ExpandoObject();
        baseSettings.Timeout = 1;
        baseSettings.Colour = "Red";
        baseSettings.SubSettings = new ExpandoObject();
        baseSettings.SubSettings.SubTimeout = 11;
        baseSettings.SubSettings.SubColour = "Green";

        // Create another object we will map onto the one above. 
        // Notice that we do not set a Colour or SubColour property.
        dynamic overrideSettings = new ExpandoObject();
        overrideSettings.Timeout = 2;
        overrideSettings.SubSettings = new ExpandoObject();
        overrideSettings.SubSettings.SubTimeout = 22;

        _settings = Mapper.Map<Settings>((object)baseSettings);
        Mapper.Map((object)overrideSettings, _settings);
    }

    [Fact]
    public void Should_work()
    {
        _settings.Timeout.ShouldBe(2);
        _settings.SubSettings.SubTimeout.ShouldBe(22);
    }
}