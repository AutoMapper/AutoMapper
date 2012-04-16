using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace AutoMapper.Modules
{
    public abstract class AutoMapperModule
    {
        public static Func<IConfiguration> Configuration = () => Mapper.Configuration;

        public abstract void Load();

        protected IMappingExpression<TSource, TDestination> CreateMap<TSource, TDestination>()
        {
            return Configuration().CreateMap<TSource, TDestination>();
        }

        public virtual string Name
        {
            get { return GetType().FullName; }
        }
    }
}
