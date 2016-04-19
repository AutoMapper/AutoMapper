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

        public AutoMapperMappingException(string message, Exception inner)
            : base(null, inner)
        {
            _message = message;
        }

        public AutoMapperMappingException(ResolutionContext context)
        {
            Context = context;
            Types = context.Types;
        }

        public AutoMapperMappingException(ResolutionContext context, Exception inner)
            : base(null, inner)
        {
            Context = context;
            Types = context.Types;
        }

        public AutoMapperMappingException(ResolutionContext context, Exception inner, PropertyMap propertyMap)
            : base(null, inner)
        {
            Context = context;
            Types = context.Types;
            PropertyMap = propertyMap;
        }

        public AutoMapperMappingException(ResolutionContext context, string message)
            : this(context)
        {
            _message = message;
        }

        public AutoMapperMappingException(TypePair types)
        {
            Types = types;
        }

        public AutoMapperMappingException(TypePair types, Exception inner)
            : base(null, inner)
        {
            Types = types;
        }

        public AutoMapperMappingException(TypePair types, string message)
            : this(types)
        {
            _message = message;
        }

        public ResolutionContext Context { get; }
        public TypePair Types { get; }
        public PropertyMap PropertyMap { get; set; }

        public override string Message
        {
            get
            {
                string message = null;
                var newLine = Environment.NewLine;
                if (Types.SourceType != null && Types.DestinationType != null)
                {
                    message = _message + newLine + newLine + "Mapping types:";
                    message += newLine +
                               $"{Types.SourceType.Name} -> {Types.DestinationType.Name}";
                    message += newLine +
                               $"{Types.SourceType.FullName} -> {Types.DestinationType.FullName}";
                }
                if (Context != null)
                { 
                    var destPath = GetDestPath();
                    message += newLine + newLine + "Destination path:" + newLine + destPath;

                    message += newLine + newLine + "Source value:" + newLine + (Context.SourceValue ?? "(null)");

                    return message;
                }
                if (_message != null)
                {
                    message = _message;
                }

                message = (message == null ? null : message + newLine) + base.Message;

                return message;
            }
        }

        private string GetDestPath()
        {
            var allContexts = GetExceptions().ToArray();

            var context = allContexts[0].Context?.Parent ?? allContexts[0].Context;
            var builder = new StringBuilder(context?.DestinationType.Name);

            foreach (var memberName in allContexts.Select(ctxt => ctxt?.PropertyMap?.DestinationProperty?.Name).Where(memberName => !string.IsNullOrEmpty(memberName)))
            {
                builder.Append(".");
                builder.Append(memberName);
            }
            return builder.ToString();
        }

        private IEnumerable<AutoMapperMappingException> GetExceptions()
        {
            Exception exc = this;
            while (exc != null)
            {
                var mappingEx = exc as AutoMapperMappingException;
                if (mappingEx != null)
                    yield return mappingEx;
                exc = exc.InnerException;
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
