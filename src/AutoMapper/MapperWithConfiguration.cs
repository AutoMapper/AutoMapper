using System;
using System.Collections.Generic;
using System.Reflection;

namespace AutoMapper
{
    class MapperWithConfiguration : IMapper
    {
        private readonly IMappingEngine _engine;
        private readonly IConfiguration _configuration;

        public MapperWithConfiguration(ITypeMapFactory typeMapFactory, IList<IObjectMapper> mappers)
        {
            var configuration = new ConfigurationStore(typeMapFactory, mappers);
            _engine = new MappingEngine(configuration);
            _configuration = configuration;
        }

        #region IMappingEngine members

        public void Dispose()
        {
            _engine.Dispose();
        }

        public IConfigurationProvider ConfigurationProvider
        {
            get { return _engine.ConfigurationProvider; }
        }

        public TDestination Map<TDestination>(object source)
        {
            return _engine.Map<TDestination>(source);
        }

        public TDestination Map<TDestination>(object source, Action<IMappingOperationOptions> opts)
        {
            return _engine.Map<TDestination>(source, opts);
        }

        public TDestination Map<TSource, TDestination>(TSource source)
        {
            return _engine.Map<TSource, TDestination>(source);
        }

        public TDestination Map<TSource, TDestination>(TSource source, Action<IMappingOperationOptions> opts)
        {
            return _engine.Map<TSource, TDestination>(source, opts);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination)
        {
            return _engine.Map(source, destination);
        }

        public TDestination Map<TSource, TDestination>(TSource source, TDestination destination, Action<IMappingOperationOptions> opts)
        {
            return _engine.Map(source, destination, opts);
        }

        public object Map(object source, Type sourceType, Type destinationType)
        {
            return _engine.Map(source, sourceType, destinationType);
        }

        public object Map(object source, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return _engine.Map(source, sourceType, destinationType, opts);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType)
        {
            return _engine.Map(source, destination, sourceType, destinationType);
        }

        public object Map(object source, object destination, Type sourceType, Type destinationType, Action<IMappingOperationOptions> opts)
        {
            return _engine.Map(source, destination, sourceType, destinationType, opts);
        }

        public TDestination DynamicMap<TSource, TDestination>(TSource source)
        {
            return _engine.DynamicMap<TSource, TDestination>(source);
        }

        public TDestination DynamicMap<TDestination>(object source)
        {
            return _engine.DynamicMap<TDestination>(source);
        }

        public object DynamicMap(object source, Type sourceType, Type destinationType)
        {
            return _engine.DynamicMap(source, sourceType, destinationType);
        }

        public void DynamicMap<TSource, TDestination>(TSource source, TDestination destination)
        {
            _engine.DynamicMap(source, destination);
        }

        public void DynamicMap(object source, object destination, Type sourceType, Type destinationType)
        {
            _engine.DynamicMap(source, destination, sourceType, destinationType);
        }

        #endregion

        #region IConfiguration members

        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression<TValueFormatter> IFormatterExpression.AddFormatter<TValueFormatter>()
        {
            return _configuration.AddFormatter<TValueFormatter>();
        }

        [Obsolete("Formatters should not be used.")]
        IFormatterCtorExpression IFormatterExpression.AddFormatter(Type valueFormatterType)
        {
            return _configuration.AddFormatter(valueFormatterType);
        }

        [Obsolete("Formatters should not be used.")]
        void IFormatterExpression.AddFormatter(IValueFormatter formatter)
        {
            _configuration.AddFormatter(formatter);
        }

        [Obsolete("Formatters should not be used.")]
        void IFormatterExpression.AddFormatExpression(Func<ResolutionContext, string> formatExpression)
        {
            _configuration.AddFormatExpression(formatExpression);
        }

        [Obsolete("Formatters should not be used.")]
        void IFormatterExpression.SkipFormatter<TValueFormatter>()
        {
            _configuration.SkipFormatter<TValueFormatter>();
        }

        [Obsolete("Formatters should not be used.")]
        IFormatterExpression IFormatterExpression.ForSourceType<TSource>()
        {
            return _configuration.ForSourceType<TSource>();
        }

        public INamingConvention SourceMemberNamingConvention
        {
            get { return _configuration.SourceMemberNamingConvention; }
            set { _configuration.SourceMemberNamingConvention = value; }
        }

        public INamingConvention DestinationMemberNamingConvention
        {
            get { return _configuration.DestinationMemberNamingConvention; }
            set { _configuration.DestinationMemberNamingConvention = value; }
        }

        public IEnumerable<string> Prefixes
        {
            get { return _configuration.Prefixes; }
        }

        public IEnumerable<string> Postfixes
        {
            get { return _configuration.Postfixes; }
        }

        public IEnumerable<string> DestinationPrefixes
        {
            get { return _configuration.DestinationPrefixes; }
        }

        public IEnumerable<string> DestinationPostfixes
        {
            get { return _configuration.DestinationPostfixes; }
        }

        public IEnumerable<AliasedMember> Aliases
        {
            get { return _configuration.Aliases; }
        }

        public bool ConstructorMappingEnabled
        {
            get { return _configuration.ConstructorMappingEnabled; }
        }

        public bool DataReaderMapperYieldReturnEnabled
        {
            get { return _configuration.DataReaderMapperYieldReturnEnabled; }
        }

        public IEnumerable<MethodInfo> SourceExtensionMethods
        {
            get { return _configuration.SourceExtensionMethods; }
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return _configuration.CreateMap<TSource, TDestination>();
        }

        public IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList memberList)
        {
            return _configuration.CreateMap<TSource, TDestination>(memberList);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType)
        {
            return _configuration.CreateMap(sourceType, destinationType);
        }

        public IMappingExpression CreateMap(Type sourceType, Type destinationType, MemberList memberList)
        {
            return _configuration.CreateMap(sourceType, destinationType, memberList);
        }

        public void RecognizePrefixes(params string[] prefixes)
        {
            _configuration.RecognizePrefixes(prefixes);
        }

        public void RecognizePostfixes(params string[] postfixes)
        {
            _configuration.RecognizePostfixes(postfixes);
        }

        public void RecognizeAlias(string original, string alias)
        {
            _configuration.RecognizeAlias(original, alias);
        }

        public void RecognizeDestinationPrefixes(params string[] prefixes)
        {
            _configuration.RecognizeDestinationPrefixes(prefixes);
        }

        public void RecognizeDestinationPostfixes(params string[] postfixes)
        {
            _configuration.RecognizeDestinationPostfixes(postfixes);
        }

        public void AddGlobalIgnore(string propertyNameStartingWith)
        {
            _configuration.AddGlobalIgnore(propertyNameStartingWith);
        }

        public bool AllowNullDestinationValues
        {
            get { return _configuration.AllowNullDestinationValues; }
            set { _configuration.AllowNullDestinationValues = value; }
        }

        public bool AllowNullCollections
        {
            get { return _configuration.AllowNullCollections; }
            set { _configuration.AllowNullCollections = value; }
        }

        public void IncludeSourceExtensionMethods(Assembly assembly)
        {
            _configuration.IncludeSourceExtensionMethods(assembly);
        }

        public IProfileExpression CreateProfile(string profileName)
        {
            return _configuration.CreateProfile(profileName);
        }

        public void CreateProfile(string profileName, Action<IProfileExpression> profileConfiguration)
        {
            _configuration.CreateProfile(profileName, profileConfiguration);
        }

        public void AddProfile(Profile profile)
        {
            _configuration.AddProfile(profile);
        }

        public void AddProfile<TProfile>() where TProfile : Profile, new()
        {
            _configuration.AddProfile<TProfile>();
        }

        public void ConstructServicesUsing(Func<Type, object> constructor)
        {
            _configuration.ConstructServicesUsing(constructor);
        }

        public void DisableConstructorMapping()
        {
            _configuration.DisableConstructorMapping();
        }

        public void Seal()
        {
            _configuration.Seal();
        }

        public void EnableYieldReturnForDataReaderMapper()
        {
            _configuration.EnableYieldReturnForDataReaderMapper();
        }

        #endregion
    }
}
