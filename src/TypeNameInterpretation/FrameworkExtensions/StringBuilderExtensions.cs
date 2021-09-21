#if !NETSTANDARD2_1_OR_GREATER && !NETCOREAPP3_1_OR_GREATER
namespace System.Text
{
	static class StringBuilderExtensions
	{
		public static unsafe StringBuilder Append(this StringBuilder builder, ReadOnlySpan<char> text)
		{
			fixed (char* c = text)
			{
				builder.Append(c, text.Length);
			}

			return builder;
		}
	}
}
#endif
