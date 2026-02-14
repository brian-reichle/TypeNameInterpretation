// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
#if !NETSTANDARD2_1_OR_GREATER && !NET
namespace System.Text;

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
#endif
