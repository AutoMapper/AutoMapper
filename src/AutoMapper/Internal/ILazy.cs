namespace AutoMapper.Internal
{
    public interface ILazy<T>
    {
        T Value { get; }
    }
}