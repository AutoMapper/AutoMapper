using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;

namespace AutoMapper
{
#if !SILVERLIGHT
	[Serializable]
#endif
    public class AutoMapperMappingException : Exception
    {
        private string _message;

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
        }

        public AutoMapperMappingException(ResolutionContext context, Exception inner)
            : base(null, inner)
        {
            Context = context;
        }

#if !SILVERLIGHT
		protected AutoMapperMappingException(
			SerializationInfo info,
			StreamingContext context) : base(info, context)
		{
		}
#endif

        public AutoMapperMappingException(ResolutionContext context, string message)
            : this(context)
        {
            _message = message;
        }

        public ResolutionContext Context { get; private set; }

        public override string Message
        {
            get
            {
                string message = null;
                if (Context != null)
                {

                    message = _message + "\n\nMapping types:";
                    message += Environment.NewLine + string.Format("{0} -> {1}", Context.SourceType.Name, Context.DestinationType.Name);
                    message += Environment.NewLine + string.Format("{0} -> {1}", Context.SourceType.FullName, Context.DestinationType.FullName);

                    var destPath = GetDestPath(Context);
                    message += "\n\nDestination path:\n" + destPath;

                    message += "\n\nSource value:\n" + (Context.SourceValue ?? "(null)");

                    return message;
                }
                if (_message != null)
                {
                    message = _message;
                }

                message = (message == null ? null : message + "\n") + base.Message;

                return message;
            }
        }

	    private string GetDestPath(ResolutionContext context)
	    {
	        var allContexts = GetContexts(context).Reverse();

	        var builder = new StringBuilder(allContexts.First().DestinationType.Name);

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

	    private static IEnumerable<ResolutionContext> GetContexts(ResolutionContext context)
	    {
            while (context.Parent != null)
            {
                yield return context;

                context = context.Parent;
            }
	        yield return context;
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
