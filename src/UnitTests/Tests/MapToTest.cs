using System;
using System.Collections;
using Xunit;

namespace AutoMapper.UnitTests.Tests
{
    /// <summary>
    /// 对象扩展类
    /// </summary>
    public static class ObjectExtensions
    {
        /// <summary>
        /// 对象映射
        /// </summary>
        /// <typeparam name="T">映射到指定类型</typeparam>
        /// <param name="source">对象</param>
        /// <returns></returns>
        public static T MapTo<T>(this object source)
        {
            if (source == null)
            {
                throw new System.ArgumentNullException(nameof(source));
            }
            var cfg = new MapperConfiguration(config =>
            {
                if (source is IEnumerable listSource)
                {
                    foreach (var item in listSource)
                    {
                        config.CreateMap(item.GetType(), typeof(T));
                        break;
                    }
                }
                else
                {
                    config.CreateMap(source.GetType(), typeof(T));
                }

                config.CreateMissingTypeMaps = true;
                config.ValidateInlineMaps = false;

                //? 目标测试属性
                config.AllowNullDestinationValues = false;
            });

            var mapper = cfg.CreateMapper();

            return mapper.Map<T>(source);
        }
    }
    /// <summary>
    /// 字符串映射对象
    /// </summary>
    public class StringPropMap
    {
        /// <summary>
        /// 主键
        /// </summary>
        public long Id { get; set; }
        /// <summary>
        /// 名称
        /// </summary>
        public string Name { get; set; }
    }

    /// <summary>
    /// MapTo测试
    /// </summary>
    public class MapToTest
    {
        [Fact]
        public void MapTo()
        {
            var source = new
            {
                Id = DateTime.Now.Ticks
            };

            var value = source.MapTo<StringPropMap>();
        }
    }
}
