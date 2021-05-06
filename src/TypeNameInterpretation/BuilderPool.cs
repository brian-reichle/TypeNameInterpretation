using System.Text;
using System.Threading;

namespace TypeNameInterpretation
{
	static class BuilderPool
	{
		public static StringBuilder Rent()
			=> Interlocked.Exchange(ref _cache, null) ?? new StringBuilder();

		public static StringBuilder Rent(int capacity)
		{
			var result = Interlocked.Exchange(ref _cache, null);

			if (result == null)
			{
				return new StringBuilder(capacity);
			}

			result.EnsureCapacity(capacity);
			return result;
		}

		public static void Return(StringBuilder builder)
		{
			builder.Length = 0;
			Interlocked.CompareExchange(ref _cache, builder, null);
		}

		public static string ToStringAndReturn(this StringBuilder builder)
		{
			var result = builder.ToString();
			Return(builder);
			return result;
		}

		static StringBuilder? _cache;
	}
}
