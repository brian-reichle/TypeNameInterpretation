// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TypeNameInterpretation;

public static class InsAssemblyExtensions
{
	public static bool TryGetVersion(this InsAssembly assembly, [NotNullWhen(true)] out Version? version)
	{
		ArgumentNullException.ThrowIfNull(assembly);

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
		ArgumentNullException.ThrowIfNull(assembly);

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
		ArgumentNullException.ThrowIfNull(assembly);

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
		ArgumentNullException.ThrowIfNull(assembly);

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
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(name);

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
	{
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(version);
		return assembly.WithQualification(WellKnownQualificationNames.Version, version.ToString());
	}

	public static InsAssembly WithPublicKey(this InsAssembly assembly, ReadOnlySpan<byte> publicKey)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.PublicKey, Convert.ToHexString(publicKey));
	}

	public static InsAssembly WithPublicKey(this InsAssembly assembly, byte[]? publicKey)
	{
		ArgumentNullException.ThrowIfNull(assembly);

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
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.PublicKeyToken, Convert.ToHexString(publicKeyToken));
	}

	public static InsAssembly WithPublicKeyToken(this InsAssembly assembly, byte[]? publicKeyToken)
	{
		ArgumentNullException.ThrowIfNull(assembly);

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
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.ProcessorArchitecture, processorArchitecture.ToString());
	}

	public static InsAssembly WithQualification(this InsAssembly assembly, string name, string value)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(name);
		return assembly.WithQualifications(assembly.Qualifications.WithQualification(name, value));
	}

	public static InsAssembly WithoutQualification(this InsAssembly assembly, string name)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(name);
		return assembly.WithQualifications(assembly.Qualifications.WithoutQualification(name));
	}

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

		if (Convert.FromHexString(value, result, out _, out _) != OperationStatus.Done)
		{
			blob = null;
			return false;
		}

		blob = result;
		return true;
	}

	const string NullBlob = "null";
}
