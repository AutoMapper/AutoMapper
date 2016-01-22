namespace AutoMapper
{
    using System;

    public interface IConfiguration : IProfileExpression
    {
        /// <summary>
        /// Create a named profile for grouped mapping configuration
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <returns>Profile configuration options</returns>
        IProfileExpression CreateProfile(string profileName);

        /// <summary>
        /// Create a named profile for grouped mapping configuration, and configure the profile
        /// </summary>
        /// <param name="profileName">Profile name</param>
        /// <param name="profileConfiguration">Profile configuration callback</param>
        void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration);

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
        /// Supply a factory method callback for creating formatters, resolvers and type converters
        /// </summary>
        /// <param name="constructor">Factory method</param>
        void ConstructServicesUsing(Func<Type, object> constructor);

        void ForAllMaps(string profileName, Action<TypeMap, IMappingExpression> configuration);
        IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(string profileName);
    }
}