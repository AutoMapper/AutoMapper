namespace AutoMapper.Internal
{
    public delegate object LateBoundMethod(object target, object[] arguments);

    public delegate object LateBoundPropertyGet(object target);

    public delegate object LateBoundFieldGet(object target);

    public delegate void LateBoundFieldSet(object target, object value);

    public delegate void LateBoundPropertySet(object target, object value);

    public delegate void LateBoundValueTypeFieldSet(ref object target, object value);

    public delegate void LateBoundValueTypePropertySet(ref object target, object value);

    public delegate object LateBoundCtor();

    public delegate object LateBoundParamsCtor(params object[] parameters);
}