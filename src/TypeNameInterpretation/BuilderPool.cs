// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Text;

namespace TypeNameInterpretation;

static class BuilderPool
{
	public static StringBuilder Rent()
	{
		var result = _cache ?? new StringBuilder();
		_cache = null;
		return result;
	}

	public static StringBuilder Rent(int capacity)
	{
		var result = _cache;
		_cache = null;

		if (result == null)
		{
			return new StringBuilder(capacity);
		}
		else
		{
			result.EnsureCapacity(capacity);
			return result;
		}
	}

	public static void Return(StringBuilder builder)
	{
		if (builder.Capacity > 512)
		{
			return;
		}

		var existing = _cache;

		if (existing == null || existing.Capacity < builder.Capacity)
		{
			builder.Length = 0;
			_cache = builder;
		}
	}

	public static string ToStringAndReturn(this StringBuilder builder)
	{
		var result = builder.ToString();
		Return(builder);
		return result;
	}

	[ThreadStatic]
	static StringBuilder? _cache;
}
