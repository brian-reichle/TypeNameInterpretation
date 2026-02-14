// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Buffers;

namespace TypeNameInterpretation;

static class Delimiters
{
#if NET
	public static SearchValues<char> All { get; } = SearchValues.Create(_allDelimiterChars);
	public static SearchValues<char> Quote { get; } = SearchValues.Create(_allDelimiterChars.AsSpan(0, 2));
#else
	public static ReadOnlySpan<char> All => _allDelimiterChars.AsSpan();
	public static ReadOnlySpan<char> Quote => _allDelimiterChars.AsSpan(0, 2);
#endif

	const string _allDelimiterChars = "\\\"[],+&*=";
}
