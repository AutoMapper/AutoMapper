namespace AutoMapper
{
	internal static class PrimitiveExtensions
	{
		public static string ToNullSafeString(this object value)
		{
			return value == null ? string.Empty : value.ToString();
		}
	}
}