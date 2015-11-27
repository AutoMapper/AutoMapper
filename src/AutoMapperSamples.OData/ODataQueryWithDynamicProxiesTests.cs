using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using NUnit.Framework;

namespace AutoMapperSamples.OData
{
    [TestFixture]
    public class ODataQueryWithDynamicProxiesTests : ODataQueryTests
    {
        protected override bool EnableDynamicProxies { get { return true; } }
    }
}
