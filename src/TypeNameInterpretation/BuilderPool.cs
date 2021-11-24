// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
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
			if (builder.Capacity > 512)
			{
				return;
			}

			builder.Length = 0;
			var existing = _cache;

			while (existing == null || existing.Capacity < builder.Capacity)
			{
				var tmp = Interlocked.CompareExchange(ref _cache, builder, existing);

				if (tmp == existing)
				{
					return;
				}

				existing = tmp;
			}
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
