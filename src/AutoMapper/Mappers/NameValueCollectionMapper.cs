#if !PORTABLE
namespace AutoMapper.Mappers
{
    using System.Collections.Specialized;

    public class NameValueCollectionMapper : IObjectMapper<NameValueCollection, NameValueCollection>
    {
        public NameValueCollection Map(NameValueCollection source, NameValueCollection destination, ResolutionContext context)
        {
            if (context.SourceValue == null)
                return null;

            var nvc = new NameValueCollection();
            foreach (var s in source.AllKeys)
                nvc.Add(s, source[s]);

            return nvc;
        }
    }
}
#endif