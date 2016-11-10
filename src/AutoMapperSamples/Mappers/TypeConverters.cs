using System.ComponentModel;
using NUnit.Framework;

namespace AutoMapperSamples.Mappers
{
    namespace TypeConverters
    {
        [TestFixture]
        public class CustomTypeConvertersExample
        {
            public class CustomConverter : TypeConverter
            {
            }
        }
    }
}