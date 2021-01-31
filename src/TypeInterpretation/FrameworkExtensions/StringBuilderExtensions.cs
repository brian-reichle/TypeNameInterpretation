#if !NETSTANDARD2_1
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
