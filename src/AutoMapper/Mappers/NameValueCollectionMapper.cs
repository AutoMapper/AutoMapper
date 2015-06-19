#if NET4 || MONODROID || MONOTOUCH || __IOS__ || DNXCORE50
namespace AutoMapper.Mappers
{
    using System.Collections.Specialized;

    /// <summary>
    /// 
    /// </summary>
    public class NameValueCollectionMapper : IObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public object Map(ResolutionContext context)
        {
            if (!IsMatch(context) || context.SourceValue == null)
                return null;

            var nvc = new NameValueCollection();
            var source = context.SourceValue as NameValueCollection;
            foreach (var s in source.AllKeys)
                nvc.Add(s, source[s]);

            return nvc;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        public bool IsMatch(ResolutionContext context)
        {
            return context.SourceType == typeof (NameValueCollection)
                   && context.DestinationType == typeof (NameValueCollection);
        }
    }
}

#endif