using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AutoMapper.UnitTests.Projection;
public class ProjectSubqueryWithParameterAsMethodArgument
{
    private MapperConfiguration _config;

    public static string MyConversionFunction(int value) {
        return (value * 2).ToString();
    }

    public static TProperty EF_PropertyMock<TProperty>(object entity, string propertyName) {
        return default(TProperty);
    }

    public ProjectSubqueryWithParameterAsMethodArgument()
    {
        _config = new MapperConfiguration(cfg => {
            cfg.CreateMap<int, string>()
                .ConvertUsing(i => MyConversionFunction(i));

            cfg.CreateMap<Entity, EntityDto>()
                .ForMember(e => e.IntToStringShadow, c => c.MapFrom(e => EF_PropertyMock<int>(e, "IntToStringShadow")))
                .ForMember(e => e.MyStringShadow, c => c.MapFrom(e => EF_PropertyMock<string>(e, "MyStringShadow")));
        });
    }

    [Fact]
    public void ShouldReplaceAllParametersCorrectly() {
        var entities = Array.Empty<Entity>().AsQueryable();

        LambdaExpression letLambda = ((entities.ProjectTo(typeof(EntityDto), _config).Expression as MethodCallExpression)
            .Arguments.Skip(1).First() as UnaryExpression)
                ?.Operand as LambdaExpression;
        letLambda.ShouldNotBeNull();
        typeof(Exception).ShouldNotBeThrownBy(() => new ParameterVisitorThrowing(letLambda.Parameters.First()).Visit(letLambda.Body));
    }

    public class ParameterVisitorThrowing : ExpressionVisitor {
        Expression _parameter;

        public ParameterVisitorThrowing(Expression parameter) {
            _parameter = parameter;
        }

        protected override Expression VisitParameter(ParameterExpression node) {
            if (node != _parameter)
                throw new Exception("Different parameter found");
            return base.VisitParameter(node);
        }
    }

    public class Entity {
        public int Id { get; set; }

        public int IntToStringRegular { get; set; }

        // int IntToStringShadow property on DB

        // string MyStringShadow property on DB
    }

    public class EntityDto {
        public int Id { get; set; }

        public string IntToStringRegular { get; set; }

        public string IntToStringShadow { get; set; }

        public string MyStringShadow { get; set; }
    }
}
