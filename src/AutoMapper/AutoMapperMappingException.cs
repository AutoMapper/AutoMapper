namespace AutoMapper
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Text;

    public class AutoMapperMappingException : Exception
    {
        private readonly string _message;

        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public AutoMapperMappingException()
        {
        }

        public AutoMapperMappingException(string message)
            : base(message)
        {
            _message = message;
        }

        public AutoMapperMappingException(string message, Exception innerException)
            : base(message, innerException)
        {
            _message = message;
        }

        public AutoMapperMappingException(string message, Exception innerException, TypePair types)
            : this(message, innerException)
        {
            Types = types;
        }

        public AutoMapperMappingException(string message, Exception innerException, TypePair types, TypeMap typeMap)
            : this(message, innerException, types)
        {
            TypeMap = typeMap;
        }

        public AutoMapperMappingException(string message, Exception innerException, TypePair types, TypeMap typeMap, PropertyMap propertyMap)
            : this(message, innerException, types, typeMap)
        {
            PropertyMap = propertyMap;
        }

        public TypePair? Types { get; set; }
        public TypeMap TypeMap { get; set; }
        public PropertyMap PropertyMap { get; set; }

        public override string Message
        {
            get
            {
                string message = _message;
                var newLine = Environment.NewLine;
                if (Types?.SourceType != null && Types?.DestinationType != null)
                {
                    message = message + newLine + newLine + "Mapping types:";
                    message += newLine +
                               $"{Types?.SourceType.Name} -> {Types?.DestinationType.Name}";
                    message += newLine +
                               $"{Types?.SourceType.FullName} -> {Types?.DestinationType.FullName}";
                }
                if (TypeMap != null)
                {
                    message = message + newLine + newLine + "Type Map configuration:";
                    message += newLine +
                               $"{TypeMap.SourceType.Name} -> {TypeMap.DestinationType.Name}";
                    message += newLine +
                               $"{TypeMap.SourceType.FullName} -> {TypeMap.DestinationType.FullName}";
                }
                if (PropertyMap != null)
                {
                    message = message + newLine + newLine + "Property:";
                    message += newLine +
                               $"{PropertyMap.DestinationProperty.Name}";
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
