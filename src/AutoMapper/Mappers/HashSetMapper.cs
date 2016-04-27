namespace AutoMapper.Mappers
{
    using System.Collections.Generic;
    using System.Linq;

    public class HashSetMapper<TSource, TDestination> : IObjectMapper<IEnumerable<TSource>, ISet<TDestination>>
    {
        public ISet<TDestination> Map(IEnumerable<TSource> source, ISet<TDestination> destination, ResolutionContext context)
        {
            if (source == null && context.Mapper.ShouldMapSourceCollectionAsNull(context))
            {
                return null;
            }

            destination = destination ?? new HashSet<TDestination>();

            destination.Clear();

            foreach (var item in source ?? Enumerable.Empty<TSource>())
            {
                destination.Add(context.Mapper.Map(item, default(TDestination), context));
            }

            return destination;
        }
    }
}