using System;
using System.Linq;
using System.Text;

namespace AutoMapper
{
    public class AutoMapperConfigurationException : Exception
    {
        public TypeMapConfigErrors[] Errors { get; }
        public TypePair? Types { get; }
        public IMemberMap MemberMap { get; set; }

        public class TypeMapConfigErrors
        {
            public TypeMap TypeMap { get; }
            public string[] UnmappedPropertyNames { get; }
            public bool CanConstruct { get; }

            public TypeMapConfigErrors(TypeMap typeMap, string[] unmappedPropertyNames, bool canConstruct)
            {
                TypeMap = typeMap;
                UnmappedPropertyNames = unmappedPropertyNames;
                CanConstruct = canConstruct;
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

        public AutoMapperConfigurationException(TypeMapConfigErrors[] errors) => Errors = errors;

        public AutoMapperConfigurationException(TypePair types) => Types = types;

        public override string Message
        {
            get
            {
                if (Types != null)
                {
                    var message =
                        string.Format(
                            "The following member on {0} cannot be mapped: \n\t{2} \nAdd a custom mapping expression, ignore, add a custom resolver, or modify the destination type {1}.",
                            Types?.DestinationType.FullName, Types?.DestinationType.FullName,
                            MemberMap?.DestinationName);

                    message += "\nContext:";

                    Exception exToUse = this;
                    while (exToUse != null)
                    {
                        if (exToUse is AutoMapperConfigurationException configExc)
                        {
                            message += configExc.MemberMap == null
                              ? $"\n\tMapping from type {configExc.Types?.SourceType.FullName} to {configExc.Types?.DestinationType.FullName}"
                              : $"\n\tMapping to member {configExc.MemberMap.DestinationName} from {configExc.Types?.SourceType.FullName} to {configExc.Types?.DestinationType.FullName}";
                        }

                        exToUse = exToUse.InnerException;
                    }

                    return message + "\n" + base.Message;
                }
                if (Errors != null)
                {
                    var message =
                        new StringBuilder(
                            "\nUnmapped members were found. Review the types and members below.\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the source/destination type\nFor no matching constructor, add a no-arg ctor, add optional arguments, or map all of the constructor parameters\n");

                    foreach (var error in Errors)
                    {
                        var len = error.TypeMap.SourceType.FullName.Length +
                                  error.TypeMap.DestinationType.FullName.Length + 5;

                        message.AppendLine(new string('=', len));
                        message.AppendLine(error.TypeMap.SourceType.Name + " -> " + error.TypeMap.DestinationType.Name +
                                           " (" +
                                           error.TypeMap.ConfiguredMemberList + " member list)");
                        message.AppendLine(error.TypeMap.SourceType.FullName + " -> " +
                                           error.TypeMap.DestinationType.FullName + " (" +
                                           error.TypeMap.ConfiguredMemberList + " member list)");
                        message.AppendLine();

                        if (error.UnmappedPropertyNames.Any())
                        {
                            message.AppendLine("Unmapped properties:");
                            foreach (var name in error.UnmappedPropertyNames)
                            {
                                message.AppendLine(name);
                            }
                        }
                        if (!error.CanConstruct)
                        {
                            message.AppendLine("No available constructor.");
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
                            .Where(str => !str.TrimStart().StartsWith("at AutoMapper."))
                            .ToArray());

                return base.StackTrace;
            }
        }
    }
}