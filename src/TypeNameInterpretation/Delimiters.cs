// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation
{
	static class Delimiters
	{
		public static ReadOnlySpan<char> All => _allDelimiterChars;
		public static ReadOnlySpan<char> Quote => _allDelimiterChars.AsSpan(0, 2);

		static readonly char[] _allDelimiterChars = { '\\', '"', '[', ']', ',', '+', '&', '*', '=' };
	}
}
