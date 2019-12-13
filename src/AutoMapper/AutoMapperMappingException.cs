using System;
#if !DEBUG
using System.Linq;
#endif

namespace AutoMapper
{
    /// <summary>
    /// Wraps mapping exceptions. Check exception.ToString() for the full error message.
    /// </summary>
    public class AutoMapperMappingException : Exception
    {
        private readonly string _message;

        public AutoMapperMappingException()
        {
        }

        public AutoMapperMappingException(string message)
            : base(message) => _message = message;

        public AutoMapperMappingException(string message, Exception innerException)
            : base(message, innerException) => _message = message;

        public AutoMapperMappingException(string message, Exception innerException, TypePair types)
            : this(message, innerException) => Types = types;

        public AutoMapperMappingException(string message, Exception innerException, TypePair types, TypeMap typeMap)
            : this(message, innerException, types) => TypeMap = typeMap;

        public AutoMapperMappingException(string message, Exception innerException, TypePair types, TypeMap typeMap, IMemberMap memberMap)
            : this(message, innerException, types, typeMap) => MemberMap = memberMap;

        public TypePair? Types { get; set; }
        public TypeMap TypeMap { get; set; }
        public IMemberMap MemberMap { get; set; }

        public override string Message
        {
            get
            {
                var message = _message;
                var newLine = Environment.NewLine;
                if (Types?.SourceType != null && Types?.DestinationType != null)
                {
                    message = message + newLine + newLine + "Mapping types:";
                    message += newLine + $"{Types?.SourceType.Name} -> {Types?.DestinationType.Name}";
                    message += newLine + $"{Types?.SourceType.FullName} -> {Types?.DestinationType.FullName}";
                }
                if (TypeMap != null)
                {
                    message = message + newLine + newLine + "Type Map configuration:";
                    message += newLine + $"{TypeMap.SourceType.Name} -> {TypeMap.DestinationType.Name}";
                    message += newLine + $"{TypeMap.SourceType.FullName} -> {TypeMap.DestinationType.FullName}";
                }
                if (MemberMap != null)
                {
                    message = message + newLine + newLine + "Destination Member:";
                    message += newLine + $"{MemberMap.DestinationName}" + newLine;
                }

                return message;
            }
        }

#if !DEBUG
        public override string StackTrace
        {
            get
            {
                return string.Join(Environment.NewLine,
                    base.StackTrace
                        .Split(new[] {Environment.NewLine}, StringSplitOptions.None)
                        .Where(str => !str.TrimStart().StartsWith("at AutoMapper.")));
            }
        }
#endif
    }
}
