using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.Concurrent;

namespace AutoMapper.Modules
{
    public abstract class AutoMapperModule
    {
        public static Func<IConfiguration> Configuration = () => Mapper.Configuration;
        private static ConcurrentDictionary<string, AutoMapperModule> LoadedModules = new ConcurrentDictionary<string, AutoMapperModule>();

        public static void ResetModules()
        {
            Mapper.Reset();
            LoadedModules = new ConcurrentDictionary<string, AutoMapperModule>();
        }

        protected abstract void OnLoad();

        public void Load()
        {
            AutoMapperModule loadedModule;
            if (LoadedModules.TryGetValue(this.Name, out loadedModule))
            {
                throw new NotSupportedException("A module with the same name is already loaded");
            }
            LoadedModules.AddOrUpdate(this.Name, this, (s, m) => m);
            this.OnLoad();
        }

        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Configuration().CreateMap<TSource, TDestination>();
        }

        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>(MemberList source)
        {
            return Configuration().CreateMap<TSource, TDestination>(source);
        }

        public virtual string Name
        {
            get { return GetType().FullName; }
        }
    }
}
