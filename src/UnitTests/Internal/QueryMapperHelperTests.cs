using System;
using Xunit;
using Should;
using AutoMapper.QueryableExtensions.Impl;

namespace AutoMapper.UnitTests
{
    public class QueryMapperHelperTests
    {
        [Fact]
        public void Should_include_full_type_name_when_missing_map()
        {
            QueryMapperHelper.MissingMapException(typeof(QueryMapperHelperTests), typeof(QueryMapperHelperTests))
                .Message.ShouldStartWith("Missing map from "+typeof(QueryMapperHelperTests).FullName);
        }
    }
}
