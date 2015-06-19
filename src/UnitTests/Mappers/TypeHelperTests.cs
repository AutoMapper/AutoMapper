namespace AutoMapper.UnitTests.Mappers
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using AutoMapper.Mappers;
    using Should;
    using Xunit;

    public class TypeHelperTests
    {
        [Fact]
        public void CanReturnElementTypeOnCollectionThatImplementsTheSameGenericInterfaceMultipleTimes()
        {
            var myType = typeof (ChargeCollection);

            var elementType = myType.GetNullEnumerableElementType();

            elementType.ShouldNotBeNull();
        }

        public class Charge
        {
        }

        public interface IChargeCollection : IEnumerable<object>
        {
        }

        public class ChargeCollection : Collection<Charge>, IChargeCollection
        {
            public new IEnumerator<object> GetEnumerator()
            {
                return null;
            }
        }
    }
}