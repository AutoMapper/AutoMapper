using System;

namespace NBehave.Spec
{
    public interface IActionSpecification<T>
    {
        void WhenCalling(Action<T> action);
    }
}