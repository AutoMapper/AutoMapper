#if NET4 || MONODROID || MONOTOUCH || __IOS__
namespace AutoMapper.Mappers
{
    using System.Collections.Specialized;

    public class NameValueCollectionMapper : IObjectMapper
    {
        public object Map(ResolutionContext context, IMappingEngineRunner mapper)
        {
            if (!IsMatch(context) || context.SourceValue == null)
                return null;

            var nvc = new NameValueCollection();
            var source = context.SourceValue as NameValueCollection;
            foreach (var s in source.AllKeys)
                nvc.Add(s, source[s]);

            return nvc;
        }

        public bool IsMatch(ResolutionContext context)
        {
            return
                context.SourceType == typeof (NameValueCollection) &&
                context.DestinationType == typeof (NameValueCollection);
        }
    }
}

#endif