using System;
using System.Collections.Immutable;
using System.Text;

namespace TypeInterpretation
{
	static class InsParser
	{
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

			public InsAssembly ParseAssembly(ref int index)
			{
				AssertNotEOF(index);
				var identifier = ParseIdentifier(ref index);
				ImmutableArray<InsAssemblyQualification>.Builder? builder = null;

				while (index < _buffer.Length && _buffer[index] == ',')
				{
					index++;
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

			readonly ReadOnlySpan<char> _buffer;
		}
	}
}
