// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
#if !NET9_0_OR_GREATER
using System.Buffers;
using System.Diagnostics;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

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

			var charCount = blob.Length * 2;

			if (blob.Length <= 8)
			{
				// For short sequences, the overhead of using ArrayPool<> will wipe away all the gains.
#if NETFRAMEWORK || NETSTANDARD2_0
				unsafe
				{
					var ptr = stackalloc char[charCount];
					EncodeCore(ref MemoryMarshal.GetReference(blob), ref Unsafe.AsRef<char>(ptr), blob.Length);
					return new string(ptr, 0, charCount);
				}
#else
				Span<char> buffer = stackalloc char[charCount];
				EncodeCore(ref MemoryMarshal.GetReference(blob), ref MemoryMarshal.GetReference(buffer), blob.Length);
				return buffer.ToString();
#endif
			}
			else
			{
				var sharedArray = ArrayPool<char>.Shared.Rent(charCount);
				EncodeCore(ref MemoryMarshal.GetReference(blob), ref sharedArray[0], blob.Length);
				ArrayPool<char>.Shared.Return(sharedArray);
				return new string(sharedArray, 0, charCount);
			}
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
		=> DecoderCore(ref MemoryMarshal.GetReference(source), ref MemoryMarshal.GetReference(target), source.Length);

	static bool DecoderCore(ref char source, ref byte target, int sourceLength)
	{
		var halfByte = false;
		var highNibble = 0;

		ref var sourcePtr = ref source;
		ref var targetPtr = ref target;

		while (sourceLength > 0)
		{
			var c = sourcePtr;
			byte nibble;

			unchecked
			{
				var tmp = (uint)(c - '0');

				if (tmp <= 9)
				{
					nibble = (byte)tmp;
				}
				else
				{
					tmp = (uint)((c | '\x20') - 'a');

					if (tmp <= 5)
					{
						nibble = (byte)(tmp + 10);
					}
					else
					{
						return false;
					}
				}
			}

			if (halfByte)
			{
				targetPtr = (byte)(nibble | highNibble);
				targetPtr = ref Unsafe.Add(ref targetPtr, 1);
				halfByte = false;
			}
			else
			{
				highNibble = nibble << 4;
				halfByte = true;
			}

			sourceLength--;
			sourcePtr = ref Unsafe.Add(ref sourcePtr, 1);
		}

		return true;
	}

#if !NET
	static void EncodeCore(ref byte source, ref char target, int sourceLength)
	{
		var charLookup = "0123456789ABCDEF";

		while (sourceLength-- > 0)
		{
			var b = source;
			source = ref Unsafe.Add(ref source, 1);

			target = charLookup[b >> 4];
			target = ref Unsafe.Add(ref target, 1);
			target = charLookup[b & 0xF];
			target = ref Unsafe.Add(ref target, 1);
		}
	}
#endif
}
#endif
