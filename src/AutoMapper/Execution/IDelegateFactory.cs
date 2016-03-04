namespace AutoMapper.Execution
{
    public delegate TValue LateBoundMethod<TSource, TValue>(TSource target, object[] arguments);

    public delegate TValue LateBoundPropertyGet<TSource, TValue>(TSource target);

    public delegate TValue LateBoundFieldGet<TSource, TValue>(TSource target);

    public delegate void LateBoundFieldSet<TSource, TValue>(TSource target, TValue value);

    public delegate void LateBoundPropertySet<TSource,TValue>(TSource target, TValue value);

    public delegate void LateBoundValueTypeFieldSet(ref object target, object value);

    public delegate void LateBoundValueTypePropertySet(ref object target, object value);

    public delegate object LateBoundCtor();

    public delegate object LateBoundParamsCtor(params object[] parameters);
}