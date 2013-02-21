using System;

namespace AutoMapper
{
    public class DuplicateCallsPerMappingException : Exception
    {
        public DuplicateCallsPerMappingException(string message)
            : base(message)
        {
        }

        protected DuplicateCallsPerMappingException(string message, Exception inner)
            : base(message, inner)
        {
        }

    }
}