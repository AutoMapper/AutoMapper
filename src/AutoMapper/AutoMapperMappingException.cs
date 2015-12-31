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

        public override string Message
        {
            get
            {
                string message = null;
                var newLine = Environment.NewLine;
                if (Types != null)
                {
                    message = _message + newLine + newLine + "Mapping types:";
                    message += newLine +
                               $"{Types.SourceType.Name} -> {Types.DestinationType.Name}";
                    message += newLine +
                               $"{Types.SourceType.FullName} -> {Types.DestinationType.FullName}";
                }
                if (Context != null)
                { 
                    var destPath = GetDestPath(Context);
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

        private string GetDestPath(ResolutionContext context)
        {
            var allContexts = context.GetContexts();

            var builder = new StringBuilder(allContexts[0].DestinationType.Name);

            foreach (var ctxt in allContexts)
            {
                if (!string.IsNullOrEmpty(ctxt.MemberName))
                {
                    builder.Append(".");
                    builder.Append(ctxt.MemberName);
                }
                if (ctxt.ArrayIndex != null)
                {
                    builder.AppendFormat("[{0}]", ctxt.ArrayIndex);
                }
            }
            return builder.ToString();
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
