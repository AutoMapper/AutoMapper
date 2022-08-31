using System;
using System.Text;
using System.Linq;
using AutoMapper.Internal;

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

        public AutoMapperMappingException(string message, Exception innerException, TypeMap typeMap)
            : this(message, innerException, typeMap.Types) => TypeMap = typeMap;

        public AutoMapperMappingException(string message, Exception innerException, MemberMap memberMap)
            : this(message, innerException, memberMap.TypeMap) => MemberMap = memberMap;

        public TypePair? Types { get; set; }
        public TypeMap TypeMap { get; set; }
        public MemberMap MemberMap { get; set; }

        public override string Message
        {
            get
            {
                var message = _message;
                var newLine = Environment.NewLine;
                if (Types.HasValue && Types.Value.SourceType != null && Types.Value.DestinationType != null)
                {
                    message = message + newLine + newLine + "Mapping types:";
                    message += newLine + $"{Types.Value.SourceType.Name} -> {Types.Value.DestinationType.Name}";
                    message += newLine + $"{Types.Value.SourceType.FullName} -> {Types.Value.DestinationType.FullName}";
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
                    message += newLine + $"{MemberMap}" + newLine;
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
    public class DuplicateTypeMapConfigurationException : Exception
    {
        public TypeMapConfigErrors[] Errors { get; }

        public DuplicateTypeMapConfigurationException(TypeMapConfigErrors[] errors)
        {
            Errors = errors;
            var builder = new StringBuilder();
            builder.AppendLine("The following type maps were found in multiple profiles:");
            foreach (var error in Errors)
            {
                builder.AppendLine($"{error.Types.SourceType.FullName} to {error.Types.DestinationType.FullName} defined in profiles:");
                builder.AppendLine(string.Join(Environment.NewLine, error.ProfileNames));
            }
            builder.AppendLine("This can cause configuration collisions and inconsistent mapping.");
            builder.AppendLine("Consolidate the CreateMap calls into one profile, or set the root Internal().AllowAdditiveTypeMapCreation configuration value to 'true'.");

            Message = builder.ToString();
        }

        public class TypeMapConfigErrors
        {
            public string[] ProfileNames { get; }
            public TypePair Types { get; }

            public TypeMapConfigErrors(TypePair types, string[] profileNames)
            {
                Types = types;
                ProfileNames = profileNames;
            }
        }

        public override string Message { get; }
    }
    public class AutoMapperConfigurationException : Exception
    {
        public TypeMapConfigErrors[] Errors { get; }
        public TypePair? Types { get; }
        public MemberMap MemberMap { get; set; }

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

        public AutoMapperConfigurationException(string message, Exception inner)
            : base(message, inner)
        {
        }

        public AutoMapperConfigurationException(TypeMapConfigErrors[] errors) => Errors = errors;

        public AutoMapperConfigurationException(TypePair types) => Types = types;

        public override string Message
        {
            get
            {
                if (Types.HasValue)
                {
                    var message =
                        string.Format(
                            "The following member on {0} cannot be mapped: \n\t{2} \nAdd a custom mapping expression, ignore, add a custom resolver, or modify the destination type {1}.",
                            Types.Value.DestinationType.FullName, Types.Value.DestinationType.FullName,
                            MemberMap);

                    message += "\nContext:";

                    Exception exToUse = this;
                    while (exToUse != null)
                    {
                        if (exToUse is AutoMapperConfigurationException configExc)
                        {
                            message += configExc.MemberMap == null
                              ? $"\n\tMapping from type {configExc.Types.Value.SourceType.FullName} to {configExc.Types.Value.DestinationType.FullName}"
                              : $"\n\tMapping to member {configExc.MemberMap} from {configExc.Types.Value.SourceType.FullName} to {configExc.Types.Value.DestinationType.FullName}";
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
                            .Split(new[] { Environment.NewLine }, StringSplitOptions.None)
                            .Where(str => !str.TrimStart().StartsWith("at AutoMapper."))
                            .ToArray());

                return base.StackTrace;
            }
        }
    }
}