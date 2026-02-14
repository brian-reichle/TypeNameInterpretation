// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
#if !NET9_0_OR_GREATER
using System.Buffers;
using System.Diagnostics;
using TypeNameInterpretation;

namespace System;

static class ConvertShims
{
	extension(Convert)
	{
#if !NET
		public static string ToHexString(ReadOnlySpan<byte> blob)
		{
			if (blob.Length == 0)
			{
				return string.Empty;
			}

			var builder = BuilderPool.Rent(blob.Length * 2);
			var charLookup = "0123456789ABCDEF";

			for (var i = 0; i < blob.Length; i++)
			{
				var b = blob[i];

				builder
					.Append(charLookup[b >> 4])
					.Append(charLookup[b & 0xF]);
			}

			return builder.ToStringAndReturn();
		}
#endif

		public static OperationStatus FromHexString(ReadOnlySpan<char> source, Span<byte> destination, out int charsConsumed, out int bytesWritten)
		{
			// We don't actually care about these as we know the caller will have already provided a destination of the exact
			// size required. The out parameters only exist to match the signature.
			charsConsumed = 0;
			bytesWritten = 0;

			Debug.Assert(source.Length == destination.Length * 2, "The destination size must exactly match the required size.");

			return DecoderCore(source, destination)
				? OperationStatus.Done
				: OperationStatus.InvalidData;
		}
	}

	static bool DecoderCore(ReadOnlySpan<char> source, Span<byte> target)
	{
		var read = 0;
		var write = 0;
		var halfByte = false;
		var highNibble = 0;

		while (read < source.Length)
		{
			var nibble = CharValue(source[read++]);

			if (nibble < 0)
			{
				return false;
			}

			if (halfByte)
			{
				target[write++] = (byte)(nibble | highNibble);
				halfByte = false;
			}
			else
			{
				highNibble = nibble << 4;
				halfByte = true;
			}
		}

		return true;
	}

	static int CharValue(char c)
	{
		if (c >= '0' && c <= '9')
		{
			return c - '0';
		}
		else if (c >= 'a' && c <= 'f')
		{
			return c - 'a' + 10;
		}
		else if (c >= 'A' && c <= 'F')
		{
			return c - 'A' + 10;
		}
		else
		{
			return -1;
		}
	}
}
#endif
