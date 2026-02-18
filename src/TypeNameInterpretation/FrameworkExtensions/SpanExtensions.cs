// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

#if !NET
namespace System;

static class SpanExtensions
{
	public static bool ContainsAny(this ReadOnlySpan<char> haystack, ReadOnlySpan<char> needle)
		=> haystack.IndexOfAny(needle) >= 0;
}
#endif
