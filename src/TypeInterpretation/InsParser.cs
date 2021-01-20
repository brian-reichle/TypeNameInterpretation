using System;
using System.Collections.Immutable;
using System.Text;

namespace TypeInterpretation
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

		static void ThrowEOF() => throw new FormatException("Unexpected end of format.");
		static void ThrowUnexpected(int index) => throw new FormatException("Unexpected char at position " + index + ".");

		ref struct Context
		{
			public Context(ReadOnlySpan<char> buffer)
			{
				_buffer = buffer;
			}

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
					_buffer[index + 1] != ']' &&
					_buffer[index + 1] != ',')
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
						index++;
						var rank = 1;

						while (TryReadChar(ref index, ','))
						{
							rank++;
						}

						ReadChar(ref index, ']');
						type = new InsArrayType(type, rank);
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

				var name = _buffer[index] == '"'
					? ParseQuotedIdentifier(ref index)
					: ParseIdentifier(ref index);

				ReadChar(ref index, '=');

				var value = _buffer[index] == '"'
					? ParseQuotedIdentifier(ref index)
					: ParseIdentifier(ref index);

				return new InsAssemblyQualification(name, value);
			}

			string ParseIdentifier(ref int index)
			{
				var start = index;
				StringBuilder? builder = null;

				while (index < _buffer.Length)
				{
					var c = _buffer[index];

					switch (c)
					{
						case '*':
						case '[':
						case ']':
						case ',':
						case '&':
						case '+':
						case '=':
							goto done;

						case '\\':
							builder ??= new StringBuilder();
							builder.Append(_buffer.Slice(start, index - start));
							start = index + 1;
							index += 2;

							if (index > _buffer.Length)
							{
								ThrowEOF();
							}
							break;

						default:
							index++;
							break;
					}
				}

			done:
				var slice = _buffer.Slice(start, index - start);

				return builder == null
					? slice.ToString()
					: builder.Append(slice).ToString();
			}

			string ParseQuotedIdentifier(ref int index)
			{
				ReadChar(ref index, '"');
				StringBuilder? builder = null;
				var start = index;

				while (index < _buffer.Length)
				{
					var c = _buffer[index];

					if (c == '"')
					{
						break;
					}
					else if (c == '\\')
					{
						builder ??= new StringBuilder();
						builder.Append(_buffer.Slice(start, index - start));
						start = index + 1;
						index += 2;
					}
					else
					{
						index++;
					}
				}

				var slice = _buffer.Slice(start, index - start);

				ReadChar(ref index, '"');

				return builder == null
					? slice.ToString()
					: builder.Append(slice).ToString();
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

			readonly ReadOnlySpan<char> _buffer;
		}
	}
}
