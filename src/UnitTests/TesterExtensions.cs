using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace AutoMapper.UnitTests
{
    public static class StopgapNBehaveExtensions
    {
        public static void ShouldBeOfLength<T>(this IEnumerable<T> collection, int length)
        {
            collection.ShouldNotBeNull();
            collection.Count().ShouldBe(length);
        }
    }
}