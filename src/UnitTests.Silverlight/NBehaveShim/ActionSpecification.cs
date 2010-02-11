using System;

namespace NBehave.Spec
{
    public class ActionSpecification<T> : IActionSpecification<T>
    {
        private readonly T _value;
        private readonly Action<Exception> _assert;

        public ActionSpecification(T value, Action<Exception> assert)
        {
            _value = value;
            _assert = assert;
        }

        public void WhenCalling(Action<T> action)
        {
            Exception e = null;

            try
            {
                action(_value);
            }
            catch (Exception ex)
            {
                e = ex;
            }

            _assert.Invoke(e);
        }
    }
}