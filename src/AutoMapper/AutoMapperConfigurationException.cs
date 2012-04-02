using System;
using System.Runtime.Serialization;
using System.Text;
using System.Linq;

namespace AutoMapper
{
#if !SILVERLIGHT
    [Serializable]
#endif
    public class AutoMapperConfigurationException : Exception
    {
        public TypeMapConfigErrors[] Errors { get; private set; }
        public ResolutionContext Context { get; private set; }

        public class TypeMapConfigErrors
        {
            public TypeMap TypeMap { get; private set; }
            public string[] UnmappedPropertyNames { get; private set; }

            public TypeMapConfigErrors(TypeMap typeMap, string[] unmappedPropertyNames)
            {
                TypeMap = typeMap;
                UnmappedPropertyNames = unmappedPropertyNames;
            }
        }

        public AutoMapperConfigurationException(string message)
            : base(message)
        {
        }

        protected AutoMapperConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AutoMapperConfigurationException(TypeMapConfigErrors[] errors)
        {
            Errors = errors;
        }

        public AutoMapperConfigurationException(ResolutionContext context)
        {
            Context = context;
        }

        public override string Message
        {
            get
            {
                if (Context != null)
                {
                    var contextToUse = Context;
                    var message = string.Format("The following property on {0} cannot be mapped: \n\t{2}\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the destination type {1}.",
                        contextToUse.DestinationType.FullName, contextToUse.SourceType.FullName, contextToUse.GetContextPropertyMap().DestinationProperty.Name);

                    message += "\nContext:";

                    while (contextToUse != null)
                    {
                        message += contextToUse.GetContextPropertyMap() == null
                                    ? string.Format("\n\tMapping to type {0} from source type {1}", contextToUse.DestinationType.FullName, contextToUse.SourceType.FullName)
                                    : string.Format("\n\tMapping to property {0} of type {1} from source type {2}", contextToUse.GetContextPropertyMap().DestinationProperty.Name, contextToUse.DestinationType.FullName, contextToUse.SourceType.FullName);
                        contextToUse = contextToUse.Parent;
                    }

                    return message + "\n" + base.Message;
                }
                if (Errors != null)
                {
                    var message = new StringBuilder("\nUnmapped members were found. Review the types and members below.\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the source/destination type\n");

                    foreach (var error in Errors)
                    {
                        var len = error.TypeMap.SourceType.FullName.Length +
                                  error.TypeMap.DestinationType.FullName.Length + 5;

                        message.AppendLine(new string('=', len));
                        message.AppendLine(error.TypeMap.SourceType.Name + " -> " + error.TypeMap.DestinationType.Name + " (" +
                                           error.TypeMap.ConfiguredMemberList + " member list)");
                        message.AppendLine(error.TypeMap.SourceType.FullName + " -> " +
                                           error.TypeMap.DestinationType.FullName + " (" + 
                                           error.TypeMap.ConfiguredMemberList + " member list)");
                        message.AppendLine(new string('-', len));
                        foreach (var name in error.UnmappedPropertyNames)
                        {
                            message.AppendLine(name);
                        }
                    }
                    return message.ToString();
                }
                return base.Message;
            }
        }

        public override string StackTrace
        {
            get
            {
                if (Errors != null)
                    return string.Join(Environment.NewLine, 
                        base.StackTrace
                            .Split(new[] {Environment.NewLine}, StringSplitOptions.None)
                            .Where(str => !str.TrimStart().StartsWith("at AutoMapper.")));

                return base.StackTrace;
            }
        }
    }
}