// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

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

		public static bool TryGetProcessorArchitecture(this InsAssembly assembly, out ProcessorArchitecture processorArchitecture)
		{
			if (!assembly.TryGetQualification(WellKnownQualificationNames.ProcessorArchitecture, out var value))
			{
				processorArchitecture = default;
				return false;
			}

			if (!Enum.TryParse(value, out processorArchitecture))
			{
				throw new FormatException("ProcessorArchitecture qualification was provided, but was unrecognised.");
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

		public static InsAssembly WithVersion(this InsAssembly assembly, Version version)
			=> assembly.WithQualification(WellKnownQualificationNames.Version, version.ToString());

		public static InsAssembly WithPublicKey(this InsAssembly assembly, ReadOnlySpan<byte> publicKey)
			=> assembly.WithQualification(WellKnownQualificationNames.PublicKey, FormatBlob(publicKey));

		public static InsAssembly WithPublicKey(this InsAssembly assembly, byte[]? publicKey)
		{
			if (publicKey == null)
			{
				return assembly.WithQualification(WellKnownQualificationNames.PublicKey, NullBlob);
			}
			else
			{
				return assembly.WithPublicKey(publicKey.AsSpan());
			}
		}

		public static InsAssembly WithPublicKeyToken(this InsAssembly assembly, ReadOnlySpan<byte> publicKeyToken)
			=> assembly.WithQualification(WellKnownQualificationNames.PublicKeyToken, FormatBlob(publicKeyToken));

		public static InsAssembly WithPublicKeyToken(this InsAssembly assembly, byte[]? publicKeyToken)
		{
			if (publicKeyToken == null)
			{
				return assembly.WithQualification(WellKnownQualificationNames.PublicKeyToken, NullBlob);
			}
			else
			{
				return assembly.WithPublicKeyToken(publicKeyToken.AsSpan());
			}
		}

		public static InsAssembly WithProcessorArchitecture(this InsAssembly assembly, ProcessorArchitecture processorArchitecture)
			=> assembly.WithQualification(WellKnownQualificationNames.ProcessorArchitecture, processorArchitecture.ToString());

		public static InsAssembly WithQualification(this InsAssembly assembly, string name, string value)
			=> assembly.WithQualifications(assembly.Qualifications.WithQualification(name, value));

		public static InsAssembly WithoutQualification(this InsAssembly assembly, string name)
			=> assembly.WithQualifications(assembly.Qualifications.WithoutQualification(name));

		static InsAssembly WithQualifications(this InsAssembly assembly, ImmutableArray<InsAssemblyQualification> newQualifications)
		{
			if (newQualifications == assembly.Qualifications)
			{
				return assembly;
			}

			return new InsAssembly(assembly.Name, newQualifications);
		}

		static ImmutableArray<InsAssemblyQualification> WithQualification(this ImmutableArray<InsAssemblyQualification> qualifications, string name, string value)
		{
			for (var i = 0; i < qualifications.Length; i++)
			{
				var qualification = qualifications[i];

				if (qualification.Name == name)
				{
					if (qualification.Value == value)
					{
						return qualifications;
					}

					return qualifications.SetItem(i, new InsAssemblyQualification(name, value));
				}
			}

			return qualifications.Add(new InsAssemblyQualification(name, value));
		}

		static ImmutableArray<InsAssemblyQualification> WithoutQualification(this ImmutableArray<InsAssemblyQualification> qualifications, string name)
		{
			for (var i = 0; i < qualifications.Length; i++)
			{
				if (qualifications[i].Name == name)
				{
					return qualifications.RemoveAt(i);
				}
			}

			return qualifications;
		}

		static bool TryParseBlob(string value, out byte[]? blob)
		{
			if (value.Length == 0)
			{
				blob = Array.Empty<byte>();
				return true;
			}
			else if (value == NullBlob)
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

#if NET9_0_OR_GREATER
			if (Convert.FromHexString(value, result, out _, out _) != System.Buffers.OperationStatus.Done)
			{
				blob = null;
				return false;
			}
#else
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
#endif

			blob = result;
			return true;
		}

		static string FormatBlob(ReadOnlySpan<byte> blob)
		{
#if NET
			return Convert.ToHexString(blob);
#else
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
#endif
		}

#if !NET9_0_OR_GREATER
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
#endif

		const string NullBlob = "null";
	}
}
