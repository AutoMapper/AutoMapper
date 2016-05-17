namespace AutoMapper
{
    using System;

    public interface IMapperConfiguration : IProfileExpression, IConfiguration
    {
        
    }

    public interface IConfiguration
    {
        /// <summary>
        /// Create missing type maps during mapping, if necessary
        /// </summary>
        bool CreateMissingTypeMaps { get; set; }

        /// <summary>
        /// Add an existing profile
        /// </summary>
        /// <param name="profile">Profile to add</param>
        void AddProfile(Profile profile);

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <typeparam name="TProfile">Profile type</typeparam>
        void AddProfile<TProfile>() where TProfile : Profile, new();

        /// <summary>
        /// Add an existing profile type. Profile will be instantiated and added to the configuration.
        /// </summary>
        /// <param name="profileType">Profile type</param>
        void AddProfile(Type profileType);

        /// <summary>
        /// Supply a factory method callback for creating resolvers and type converters
        /// </summary>
        /// <param name="constructor">Factory method</param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        /// <summary>
        /// Create a named profile with the supplied configuration
        /// </summary>
        /// <param name="profileName">Profile name, must be unique</param>
        /// <param name="config">Profile configuration</param>
        void CreateProfile(string profileName, Action<Profile> config);
    }
}