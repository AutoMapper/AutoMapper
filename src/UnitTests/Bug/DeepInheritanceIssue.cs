namespace AutoMapper.UnitTests.Bug;

public class DeepInheritanceIssue
{
    [Fact]
    public void Example()
    {
        var config = new MapperConfiguration(cfg =>
        {
            cfg.CreateMap<ContainsASrc, ContainsADest>();

            cfg.CreateMap<ASrc, ADest>()
                .Include<BSrc, BDest>()
                .Include<CSrc, CDest>();

            cfg.CreateMap<BSrc, BDest>()
                .Include<CSrc, CDest>();

            cfg.CreateMap<CSrc, CDest>();
        });

        var expectedCSrc = new CSrc() {StringA = "A", StringB = "B", StringC = "C"};
        var expectedBSrc = new BSrc() {StringA = "A", StringB = "B"};

        var expectedContCSrc = new ContainsASrc() {A = expectedCSrc};
        var expectedContBSrc = new ContainsASrc() {A = expectedBSrc};

        var mapper = config.CreateMapper();
        var actualContCDest = mapper.Map<ContainsASrc, ContainsADest>(expectedContCSrc);
        var actualContBDest = mapper.Map<ContainsASrc, ContainsADest>(expectedContBSrc); // THROWS

        config.AssertConfigurationIsValid();
        actualContBDest.ShouldNotBeNull();
        actualContCDest.ShouldNotBeNull();
    }
}

public class ContainsASrc
{
    public ASrc A { get; set; }
}

public abstract class ASrc
{
    public string StringA { get; set; }
}

public class BSrc : ASrc
{
    public string StringB { get; set; }
}

public class CSrc : BSrc
{
    public string StringC { get; set; }
}

public class ContainsADest
{
    public ADest A { get; set; }
}

public abstract class ADest
{
    public string StringA { get; set; }
}

public class BDest : ADest
{
    public string StringB { get; set; }
}

public class CDest : BDest
{
    public string StringC { get; set; }
}