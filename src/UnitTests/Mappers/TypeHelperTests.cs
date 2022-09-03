namespace AutoMapper.UnitTests.Mappers;

public class TypeHelperTests
{
    [Fact]
    public void CanReturnElementTypeOnCollectionThatImplementsTheSameGenericInterfaceMultipleTimes()
    {
        Type myType = typeof(ChargeCollection);

        Type elementType = ReflectionHelper.GetElementType(myType);

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
