// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Text;

#if NET
using System.Buffers;
using System.Runtime.CompilerServices;
#endif

namespace TypeNameInterpretation;

static class InsParser
{
	public static InsType ParseType(ReadOnlySpan<char> value)
	{
		var index = 0;
		var result = new Context(value).ParseQualified(ref index);

		if (index != value.Length)
		{
			ThrowUnexpected(index);
		}

		return result;
	}

	public static InsAssembly ParseAssembly(ReadOnlySpan<char> value)
	{
		var index = 0;
		var result = new Context(value).ParseAssembly(ref index);

		if (index != value.Length)
		{
			ThrowUnexpected(index);
		}

		return result;
	}

	static void ThrowEOF() => throw new InvalidTypeNameException("Unexpected end of format.");
	static void ThrowUnexpected(int index) => throw new InvalidTypeNameException("Unexpected char at position " + index + ".");

	readonly ref struct Context(ReadOnlySpan<char> buffer)
	{
		public InsType ParseQualified(ref int index)
		{
			var assemblyStart = LocateStartOfAssembly(index);

			if (assemblyStart < 0)
			{
				return ParseUnqualified(ref index, null);
			}

			var assemblyEnd = assemblyStart + 1;
			DiscardWhitespace(ref assemblyEnd);
			var assembly = ParseAssembly(ref assemblyEnd);

			var result = ParseUnqualified(ref index, assembly);

			if (index != assemblyStart)
			{
				ThrowUnexpected(index);
			}

			index = assemblyEnd;
			return result;
		}

		InsType ParseUnqualified(ref int index, InsAssembly? assembly)
		{
			var identifier = ParseIdentifier(ref index);
			var baseType = new InsNamedType(identifier, assembly);

			while (TryReadChar(ref index, '+'))
			{
				AssertNotEOF(index);
				identifier = ParseIdentifier(ref index);
				baseType = new InsNamedType(identifier, baseType);
			}

			InsType type;

			if (index + 1 < _buffer.Length &&
				_buffer[index] == '[' &&
				_buffer[index + 1] is not ']' and not ',' and not '*')
			{
				var typeArguments = ParseTypeArguments(ref index);
				type = new InsGenericType(baseType, typeArguments);
			}
			else
			{
				type = baseType;
			}

			while (index < _buffer.Length)
			{
				var c = _buffer[index];

				if (c == '*')
				{
					type = new InsPointerType(type);
					index++;
				}
				else if (c == '[')
				{
					type = ParseArrayDetails(ref index, type);
				}
				else
				{
					break;
				}
			}

			if (TryReadChar(ref index, '&'))
			{
				type = new InsByRefType(type);
			}

			return type;
		}

		InsType ParseArrayDetails(ref int index, InsType elementType)
		{
			index++;

			if (TryReadChar(ref index, ']'))
			{
				return new InsSZArrayType(elementType);
			}

			var rank = 1;
			ParseArrayRank(ref index);

			while (TryReadChar(ref index, ','))
			{
				rank++;
				ParseArrayRank(ref index);
			}

			ReadChar(ref index, ']');
			return new InsArrayType(elementType, rank);
		}

		void ParseArrayRank(ref int index)
		{
			TryReadChar(ref index, '*');
		}

		ImmutableArray<InsType> ParseTypeArguments(ref int index)
		{
			ReadChar(ref index, '[');
			var firstType = ParseTypeArgument(ref index);

			if (TryReadChar(ref index, ']'))
			{
				return ImmutableArray.Create(firstType);
			}

			ReadChar(ref index, ',');

			var secondType = ParseTypeArgument(ref index);

			if (TryReadChar(ref index, ']'))
			{
				return ImmutableArray.Create(firstType, secondType);
			}

			ReadChar(ref index, ',');

			var builder = ImmutableArray.CreateBuilder<InsType>();
			builder.Add(firstType);
			builder.Add(secondType);

			do
			{
				builder.Add(ParseTypeArgument(ref index));
			}
			while (TryReadChar(ref index, ','));

			ReadChar(ref index, ']');
			return builder.ToImmutable();
		}

		InsType ParseTypeArgument(ref int index)
		{
			if (TryReadChar(ref index, '['))
			{
				var result = ParseQualified(ref index);
				ReadChar(ref index, ']');
				return result;
			}
			else
			{
				return ParseUnqualified(ref index, null);
			}
		}

		public InsAssembly ParseAssembly(ref int index)
		{
			// The assembly could be unqualified, partially qualified or fully qualified,
			// so there could be 0-4 qualifications. If there are more than 4 qualifications
			// we still parse them because:
			// 1. We want to faithfully represent what is actually in the input string.
			// 2. The consumer may be trying to represent additional non-standard information.
			//    (this is fine as long as they remember to strip it out before formatting
			//    a string that they then try to pass to the runtime.)
			//
			// Note: ProcessorArchitecture is optional and rarely specified. So 3 will be
			// sufficient for most cases.
			AssertNotEOF(index);
			var identifier = ParseIdentifier(ref index);

#if NET
			InlineArray4<InsAssemblyQualification> qualifications = default;
			Span<InsAssemblyQualification> qualificationSpan = qualifications;

			for (var i = 0; i < qualificationSpan.Length; i++)
			{
				if (!TryReadChar(ref index, ','))
				{
					return new InsAssembly(identifier, ImmutableArray.Create(qualificationSpan.Slice(0, i)));
				}

				DiscardWhitespace(ref index);
				qualificationSpan[i] = ParseQualification(ref index);
			}

			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, ImmutableArray.Create(qualificationSpan));
			}

			var builder = ImmutableArray.CreateBuilder<InsAssemblyQualification>();
			builder.AddRange(qualificationSpan);
#else
			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, []);
			}

			DiscardWhitespace(ref index);
			var q1 = ParseQualification(ref index);

			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, ImmutableArray.Create(q1));
			}

			DiscardWhitespace(ref index);
			var q2 = ParseQualification(ref index);

			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, ImmutableArray.Create(q1, q2));
			}

			DiscardWhitespace(ref index);
			var q3 = ParseQualification(ref index);

			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, ImmutableArray.Create(q1, q2, q3));
			}

			DiscardWhitespace(ref index);
			var q4 = ParseQualification(ref index);

			if (!TryReadChar(ref index, ','))
			{
				return new InsAssembly(identifier, ImmutableArray.Create(q1, q2, q3, q4));
			}

			var builder = ImmutableArray.CreateBuilder<InsAssemblyQualification>();
			builder.Add(q1);
			builder.Add(q2);
			builder.Add(q3);
			builder.Add(q4);

#endif

			do
			{
				DiscardWhitespace(ref index);
				builder.Add(ParseQualification(ref index));
			}
			while (TryReadChar(ref index, ','));

			return new InsAssembly(identifier, builder.ToImmutable());
		}

		InsAssemblyQualification ParseQualification(ref int index)
		{
			AssertNotEOF(index);

			var name = ParseQualificationKey(ref index);
			ReadChar(ref index, '=');
			var value = ParseQuoteableIdentifier(ref index);

			return new InsAssemblyQualification(name, value);
		}

		string ParseQualificationKey(ref int index)
		{
			if (TrySliceIdentifier(ref index, out var identifierSpan))
			{
				return identifierSpan switch
				{
					// At the time of writing, the compiler does not optimise this. Instead it generates
					// a series of sequence equality checks. So we sort them by how likely they are to
					// turn up in a partially qualified assembly name.
					WellKnownQualificationNames.Version => WellKnownQualificationNames.Version,
					WellKnownQualificationNames.PublicKeyToken => WellKnownQualificationNames.PublicKeyToken,
					WellKnownQualificationNames.Culture => WellKnownQualificationNames.Culture,
					WellKnownQualificationNames.PublicKey => WellKnownQualificationNames.PublicKey,
					WellKnownQualificationNames.ProcessorArchitecture => WellKnownQualificationNames.ProcessorArchitecture,
					_ => identifierSpan.ToString(),
				};
			}

			return ParseQuoteableIdentifier(ref index);
		}

		// Attempts to parse an identifier and return it as a ReadOnlySpan<> to avoid an allocation.
		// This only works if the identifier does not escape any characters, if it does then it
		// returns false and the caller should fallback to using ParseQuoteableIdentifier instead.
		// The `index` parameter will only be advanced if TrySliceIdentifier returns true, indicating
		// that it successfully consumed input.
		bool TrySliceIdentifier(ref int index, out ReadOnlySpan<char> identifier)
		{
			var pos = index;

			if (TryReadChar(ref pos, '"'))
			{
				if (TrySliceIdentifierCore(_buffer.Slice(pos), Delimiters.Quote, out identifier))
				{
					pos += identifier.Length;
					ReadChar(ref pos, '"');
					index = pos;
					return true;
				}
			}
			else
			{
				if (TrySliceIdentifierCore(_buffer.Slice(pos), Delimiters.All, out identifier))
				{
					pos += identifier.Length;
					index = pos;
					return true;
				}
			}

			identifier = [];
			return false;
		}

		string ParseQuoteableIdentifier(ref int index)
		{
			if (TryReadChar(ref index, '"'))
			{
				var result = ParseIdentifierCore(ref index, Delimiters.Quote);
				ReadChar(ref index, '"');
				return result;
			}

			return ParseIdentifier(ref index);
		}

		string ParseIdentifier(ref int index) => ParseIdentifierCore(ref index, Delimiters.All);

#if NET
		static bool TrySliceIdentifierCore(ReadOnlySpan<char> source, SearchValues<char> delimiters, out ReadOnlySpan<char> identifier)
#else
		static bool TrySliceIdentifierCore(ReadOnlySpan<char> source, ReadOnlySpan<char> delimiters, out ReadOnlySpan<char> identifier)
#endif
		{
			var index = 0;

			while (true)
			{
				var i = source.Slice(index).IndexOfAny(delimiters);

				if (i < 0)
				{
					index = source.Length;
					break;
				}

				index += i;

				if (source[index] != '\\')
				{
					break;
				}

				identifier = [];
				return false;
			}

			identifier = source.Slice(0, index);
			return true;
		}

#if NET
		string ParseIdentifierCore(ref int index, SearchValues<char> delimiters)
#else
		string ParseIdentifierCore(ref int index, ReadOnlySpan<char> delimiters)
#endif
		{
			AssertNotEOF(index);

			var start = index;
			StringBuilder? builder = null;

			while (true)
			{
				var i = _buffer.Slice(index).IndexOfAny(delimiters);

				if (i < 0)
				{
					index = _buffer.Length;
					break;
				}

				index += i;

				if (_buffer[index] != '\\')
				{
					break;
				}

				builder ??= BuilderPool.Rent();
				builder.Append(_buffer.Slice(start, index - start));
				start = index + 1;

				AssertNotEOF(start);
				index = start + 1;
			}

			var section = _buffer.Slice(start, index - start);

			return builder == null
				? section.ToString()
				: builder.Append(section).ToStringAndReturn();
		}

		void DiscardWhitespace(ref int index)
		{
			while (index < _buffer.Length && char.IsWhiteSpace(_buffer[index]))
			{
				index++;
			}
		}

		bool TryReadChar(ref int index, char c)
		{
			if (index < _buffer.Length && _buffer[index] == c)
			{
				index++;
				return true;
			}

			return false;
		}

		void ReadChar(ref int index, char c)
		{
			AssertNotEOF(index);

			if (_buffer[index] != c)
			{
				ThrowUnexpected(index);
			}

			index++;
		}

		void AssertNotEOF(int index)
		{
			if (index >= _buffer.Length)
			{
				ThrowEOF();
			}
		}

		int LocateStartOfAssembly(int index)
		{
			var depth = 0;
			var quoted = false;

			while (index < _buffer.Length)
			{
#if NET
				// Fast forward to the next meaningful char.
				// This generally improves the performance in .NET Core but slows down .Net Framework.
				var i = _buffer.Slice(index).IndexOfAny(Delimiters.AssemblySearch);

				if (i < 0)
				{
					return -1;
				}

				index += i;
#endif

				var c = _buffer[index];

				if (c == '\\')
				{
					index += 2;
					continue;
				}
				else if (c == '"')
				{
					quoted = !quoted;
				}
				else if (quoted)
				{
				}
				else if (c == '[')
				{
					depth++;
				}
				else if (c == ']')
				{
					depth--;

					if (depth < 0)
					{
						return -1;
					}
				}
				else if (c == ',' && depth == 0)
				{
					return index;
				}

				index++;
			}

			return -1;
		}

		readonly ReadOnlySpan<char> _buffer = buffer;
	}
}
