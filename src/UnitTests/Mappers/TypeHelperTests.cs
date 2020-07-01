using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using AutoMapper.Internal;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests.Mappers
{
    public class TypeHelperTests
    {
        [Fact]
        public void CanReturnElementTypeOnCollectionThatImplementsTheSameGenericInterfaceMultipleTimes()
        {
            Type myType = typeof(ChargeCollection);

            Type elementType = ElementTypeHelper.GetElementType(myType);

            elementType.ShouldNotBeNull();
        }

        public class Charge { }

        public interface IChargeCollection : IEnumerable<object> { }

        public class ChargeCollection : Collection<Charge>, IChargeCollection
        {
            public new IEnumerator<object> GetEnumerator()
            {
                return null;
            }
        }
    }
}
