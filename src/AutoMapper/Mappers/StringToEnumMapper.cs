using System.Runtime.Serialization;
namespace AutoMapper.Internal.Mappers;
public sealed class StringToEnumMapper : IObjectMapper
{
    private static readonly MethodInfo EqualsMethod = typeof(StringToEnumMapper).GetMethod(nameof(StringCompareOrdinalIgnoreCase));
    private static readonly MethodInfo ParseMethod = typeof(Enum).StaticGenericMethod("Parse", parametersCount: 2);
    private static readonly PropertyInfo Length = typeof(string).GetProperty("Length");
    public bool IsMatch(TypePair context) => context.SourceType == typeof(string) && context.DestinationType.IsEnum;
    public Expression MapExpression(IGlobalConfiguration configuration, ProfileMap profileMap,
        MemberMap memberMap, Expression sourceExpression, Expression destExpression)
    {
        var destinationType = destExpression.Type;
        var ignoreCase = True;
        var enumParse = Call(ParseMethod.MakeGenericMethod(destinationType), sourceExpression, ignoreCase);
        var enumMember = CheckEnumMember(sourceExpression, destinationType, enumParse, EqualsMethod);
        return Condition(Equal(Property(sourceExpression, Length), Zero), configuration.Default(destinationType), enumMember);
    }
    internal static Expression CheckEnumMember(Expression sourceExpression, Type enumType, Expression defaultExpression, MethodInfo comparison = null)
    {
        List<SwitchCase> switchCases = null;
        foreach (var memberInfo in enumType.GetFields(TypeExtensions.StaticFlags))
        {
            var attributeValue = memberInfo.GetCustomAttribute<EnumMemberAttribute>()?.Value;
            if (attributeValue == null)
            {
                continue;
            }
            var enumToObject = Constant(Enum.ToObject(enumType, memberInfo.GetValue(null)));
            var attributeConstant = Constant(attributeValue);
            var (body, testValue) = comparison == null ? (attributeConstant, enumToObject) : (ToType(enumToObject, enumType), attributeConstant);
            switchCases ??= [];
            switchCases.Add(SwitchCase(body, testValue));
        }
        return switchCases == null ? defaultExpression : Switch(sourceExpression, defaultExpression, comparison, switchCases);
    }
    public static bool StringCompareOrdinalIgnoreCase(string x, string y) => StringComparer.OrdinalIgnoreCase.Equals(x, y);
}