namespace AutoMapper
{
    using System;
    using System.Linq;
    using System.Text;

    public class AutoMapperConfigurationException : Exception
    {
        public TypeMapConfigErrors[] Errors { get; }
        public TypePair? Types { get; }
        public PropertyMap PropertyMap { get; set; }

        public class TypeMapConfigErrors
        {
            public TypeMap TypeMap { get; }
            public string[] UnmappedPropertyNames { get; }

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

        public AutoMapperConfigurationException(TypePair types)
        {
            Types = types;
        }

        public override string Message
        {
            get
            {
                if (Types != null)
                {
                    var message =
                        string.Format(
                            "The following property on {0} cannot be mapped: \n\t{2}\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the destination type {1}.",
                            Types?.DestinationType.FullName, Types?.DestinationType.FullName,
                            PropertyMap?.DestinationProperty.Name);

                    message += "\nContext:";

                    Exception exToUse = this;
                    while (exToUse != null)
                    {
                        var configExc = exToUse as AutoMapperConfigurationException;
                        if (configExc != null)
                        { message += configExc.PropertyMap == null
                            ? string.Format("\n\tMapping from type {1} to {0}", configExc.Types?.DestinationType.FullName,
                                configExc.Types?.SourceType.FullName)
                            : string.Format("\n\tMapping to property {0} from {2} to {1}",
                                configExc.PropertyMap.DestinationProperty.Name,
                                configExc.Types?.DestinationType.FullName, configExc.Types?.SourceType.FullName);
                        }

                        exToUse = exToUse.InnerException;
                    }

                    return message + "\n" + base.Message;
                }
                if (Errors != null)
                {
                    var message =
                        new StringBuilder(
                            "\nUnmapped members were found. Review the types and members below.\nAdd a custom mapping expression, ignore, add a custom resolver, or modify the source/destination type\n");

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
                        message.AppendLine("Unmapped properties:");
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
                            .Where(str => !str.TrimStart().StartsWith("at AutoMapper."))
                            .ToArray());

                return base.StackTrace;
            }
        }
    }
}