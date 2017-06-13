using System;
using System.Text;

namespace AutoMapper
{
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
            builder.AppendLine("Consolidate the CreateMap calls into one profile, or set the root Advanced.AllowAdditiveTypeMapCreation configuration value to 'true'.");

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
}