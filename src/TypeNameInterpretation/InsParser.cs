// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Text;

namespace TypeNameInterpretation
{
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
				AssertNotEOF(index);
				var identifier = ParseIdentifier(ref index);
				ImmutableArray<InsAssemblyQualification>.Builder? builder = null;

				while (TryReadChar(ref index, ','))
				{
					DiscardWhitespace(ref index);
					AssertNotEOF(index);
					builder ??= ImmutableArray.CreateBuilder<InsAssemblyQualification>();
					builder.Add(ParseQualification(ref index));
				}

				return new InsAssembly(identifier, builder?.ToImmutable() ?? ImmutableArray<InsAssemblyQualification>.Empty);
			}

			InsAssemblyQualification ParseQualification(ref int index)
			{
				AssertNotEOF(index);

				var name = ParseQuoteableIdentifier(ref index);
				ReadChar(ref index, '=');
				var value = ParseQuoteableIdentifier(ref index);

				return new InsAssemblyQualification(name, value);
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
}
