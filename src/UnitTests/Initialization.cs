using System;
using Shouldly;
using Xunit;

namespace AutoMapper.UnitTests
{
    public class Initialization
    {
        [Fact]
        public void Should_not_throw_when_resetting()
        {
            var action = new Action(() =>
            {
                Mapper.Initialize(cfg => { });
                Mapper.Reset();
                Mapper.Initialize(cfg => { });
            });
            action.ShouldNotThrow();
        }
    }
}