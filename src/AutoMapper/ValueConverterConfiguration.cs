using System;
using System.Linq.Expressions;

namespace AutoMapper
{
    public class ValueConverterConfiguration
    {
        public Type SourceType { get; set; }
        public Type DestinationType { get; set; }
        public LambdaExpression TransformerExpression { get; set; }
    }
}