namespace AutoMapper.UnitTests;

public static class AssertionExtensions
{
    public static void ShouldContain(this IEnumerable items, object item) 
        => ShouldBeEnumerableTestExtensions.ShouldContain(items.Cast<object>(), item);

    public static void ShouldBeEmpty(this IEnumerable items) 
        => ShouldBeEnumerableTestExtensions.ShouldBeEmpty(items.Cast<object>());

    public static void ShouldBeThrownBy(this Type exceptionType, Action action) 
        => action.ShouldThrow(exceptionType);

    public static void ShouldThrowException<T>(this Action action, Action<T> customAssertion) where T : Exception
    {
        bool throws = false;
        try
        {
            action();
        }
        catch (T e)
        {
            throws = true;
            customAssertion(e);
        }
        throws.ShouldBeTrue();
    }

    public static void ShouldNotBeThrownBy(this Type exceptionType, Action action) 
        => action.ShouldNotThrow();
}