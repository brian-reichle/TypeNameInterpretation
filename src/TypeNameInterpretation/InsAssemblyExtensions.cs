using System;
using System.Diagnostics.CodeAnalysis;

namespace TypeNameInterpretation
{
	public static class InsAssemblyExtensions
	{
		public static bool TryGetVersion(this InsAssembly assembly, [NotNullWhen(true)] out Version? version)
		{
			if (!assembly.TryGetQualification(WellKnownQualificationNames.Version, out var value))
			{
				version = null;
				return false;
			}

			if (!Version.TryParse(value, out version))
			{
				throw new FormatException("Version qualification was provided, but was in an unrecognised format.");
			}

			return true;
		}

		public static bool TryGetPublicKey(this InsAssembly assembly, out byte[]? publicKey)
		{
			if (!assembly.TryGetQualification(WellKnownQualificationNames.PublicKey, out var value))
			{
				publicKey = null;
				return false;
			}

			if (!TryParseBlob(value, out publicKey))
			{
				throw new FormatException("PublicKey qualification was provided, but was in an unrecognised format.");
			}

			return true;
		}

		public static bool TryGetPublicKeyToken(this InsAssembly assembly, out byte[]? publicKeyToken)
		{
			if (!assembly.TryGetQualification(WellKnownQualificationNames.PublicKeyToken, out var value))
			{
				publicKeyToken = null;
				return false;
			}

			if (!TryParseBlob(value, out publicKeyToken))
			{
				throw new FormatException("PublicKeyToken qualification was provided, but was in an unrecognised format.");
			}

			return true;
		}

		public static bool TryGetQualification(this InsAssembly assembly, string name, [NotNullWhen(true)] out string? value)
		{
			foreach (var qualification in assembly.Qualifications)
			{
				if (qualification.Name == name)
				{
					value = qualification.Value;
					return true;
				}
			}

			value = null;
			return false;
		}

		static bool TryParseBlob(string value, out byte[]? blob)
		{
			if (value.Length == 0)
			{
				blob = Array.Empty<byte>();
				return true;
			}
			else if (value == "null")
			{
				blob = null;
				return true;
			}
			else if ((value.Length & 1) == 1)
			{
				blob = null;
				return false;
			}

			var result = new byte[value.Length >> 1];

			for (var i = 0; i < result.Length; i++)
			{
				var baseIndex = i << 1;
				var n1 = CharValue(value[baseIndex]);

				if (n1 < 0)
				{
					blob = null;
					return false;
				}

				var n2 = CharValue(value[baseIndex + 1]);

				if (n2 < 0)
				{
					blob = null;
					return false;
				}

				result[i] = (byte)((n1 << 4) | n2);
			}

			blob = result;
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
}
