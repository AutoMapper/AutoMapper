namespace AutoMapper.Mappers
{
    /// <summary>
    /// 
    /// </summary>
    public interface ITypeMapObjectMapper
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        object Map(ResolutionContext context);

        /// <summary>
        /// 
        /// </summary>
        /// <param name="context"></param>
        /// <returns></returns>
        bool IsMatch(ResolutionContext context);
    }
}