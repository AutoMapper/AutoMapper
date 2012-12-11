﻿using NUnit.Framework;

namespace AutoMapper.UnitTests.Bug
{
    [TestFixture]
    public class DeepInheritanceIssue
    {
        [Test]
        public void Example()
        {
            Mapper.Initialize(cfg =>
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

            var actualContCDest = Mapper.Map<ContainsASrc, ContainsADest>(expectedContCSrc);
            var actualContBDest = Mapper.Map<ContainsASrc, ContainsADest>(expectedContBSrc); // THROWS

            Mapper.AssertConfigurationIsValid(false);
            Assert.IsNotNull(actualContBDest);
            Assert.IsNotNull(actualContCDest);
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
}