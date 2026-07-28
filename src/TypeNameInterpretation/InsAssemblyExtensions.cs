// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Buffers;
using System.Collections.Immutable;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace TypeNameInterpretation;

/// <summary>
/// Extension methods for reading and modifying qualifications on <see cref="InsAssembly"/> instances.
/// </summary>
public static class InsAssemblyExtensions
{
	/// <summary>
	/// Attempts to extract the assembly version from the assembly qualifications.
	/// </summary>
	/// <param name="assembly">The assembly to inspect.</param>
	/// <param name="version">When this method returns, contains the parsed <see cref="Version"/> if present; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the version qualification was present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	/// <exception cref="FormatException">The version qualification was present, but was not in a valid format.</exception>
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

	/// <summary>
	/// Attempts to extract the full public key byte array from the assembly qualifications.
	/// </summary>
	/// <param name="assembly">The assembly to inspect.</param>
	/// <param name="publicKey">When this method returns, contains the decoded public key bytes if present; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the public key qualification was present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	/// <exception cref="FormatException">The public key qualification was present, but was not in a valid format.</exception>
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

	/// <summary>
	/// Attempts to extract the public key token byte array from the assembly qualifications.
	/// </summary>
	/// <param name="assembly">The assembly to inspect.</param>
	/// <param name="publicKeyToken">When this method returns, contains the decoded public key token bytes if present; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the public key token qualification was present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	/// <exception cref="FormatException">The public key token qualification was present, but was not in a valid format.</exception>
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

	/// <summary>
	/// Attempts to extract the target processor architecture from the assembly qualifications.
	/// </summary>
	/// <param name="assembly">The assembly to inspect.</param>
	/// <param name="processorArchitecture">When this method returns, contains the parsed <see cref="ProcessorArchitecture"/> if present; otherwise, <see cref="ProcessorArchitecture.None"/>.</param>
	/// <returns><see langword="true"/> if the processor architecture qualification was present; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	/// <exception cref="FormatException">The processor architecture qualification was present, but was not recognized.</exception>
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

	/// <summary>
	/// Attempts to extract a qualification value by key name.
	/// </summary>
	/// <param name="assembly">The assembly to inspect.</param>
	/// <param name="name">The key name of the qualification to find.</param>
	/// <param name="value">When this method returns, contains the qualification value if found; otherwise, <see langword="null"/>.</param>
	/// <returns><see langword="true"/> if the qualification with the specified name was found; otherwise, <see langword="false"/>.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
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

	/// <summary>
	/// Returns an assembly reference with the specified version qualification set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="version">The version to set.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the specified version qualification, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> or <paramref name="version"/> is <see langword="null"/>.</exception>
	public static InsAssembly WithVersion(this InsAssembly assembly, Version version)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(version);
		return assembly.WithQualification(WellKnownQualificationNames.Version, version.ToString());
	}

	/// <summary>
	/// Returns an assembly reference with the specified public key set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="publicKey">The public key byte span.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the public key qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static InsAssembly WithPublicKey(this InsAssembly assembly, ReadOnlySpan<byte> publicKey)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.PublicKey, Convert.ToHexString(publicKey));
	}

	/// <summary>
	/// Returns an assembly reference with the specified public key set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="publicKey">The public key byte array, or <see langword="null"/> for a null blob.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the public key qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
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

	/// <summary>
	/// Returns an assembly reference with the specified public key token set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="publicKeyToken">The public key token byte span.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the public key token qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static InsAssembly WithPublicKeyToken(this InsAssembly assembly, ReadOnlySpan<byte> publicKeyToken)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.PublicKeyToken, Convert.ToHexString(publicKeyToken));
	}

	/// <summary>
	/// Returns an assembly reference with the specified public key token set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="publicKeyToken">The public key token byte array, or <see langword="null"/> for a null blob.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the public key token qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
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

	/// <summary>
	/// Returns an assembly reference with the specified processor architecture qualification set.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="processorArchitecture">The processor architecture to set.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the processor architecture qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static InsAssembly WithProcessorArchitecture(this InsAssembly assembly, ProcessorArchitecture processorArchitecture)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		return assembly.WithQualification(WellKnownQualificationNames.ProcessorArchitecture, processorArchitecture.ToString());
	}

	/// <summary>
	/// Returns an assembly reference with a qualification added or updated.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="name">The qualification key name.</param>
	/// <param name="value">The qualification value.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the qualification set, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/>, <paramref name="name"/>, or <paramref name="value"/> is <see langword="null"/>.</exception>
	public static InsAssembly WithQualification(this InsAssembly assembly, string name, string value)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(value);
		return assembly.WithQualifications(assembly.Qualifications.WithQualification(name, value));
	}

	/// <summary>
	/// Returns an assembly reference with a qualification removed.
	/// </summary>
	/// <param name="assembly">The base assembly reference.</param>
	/// <param name="name">The key name of the qualification to remove.</param>
	/// <returns>A new <see cref="InsAssembly"/> with the specified qualification removed, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
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
		var index = qualifications.IndexOfQualification(name);

		if (index < 0)
		{
			return qualifications.Add(new InsAssemblyQualification(name, value));
		}
		else if (qualifications[index].Value != value)
		{
			return qualifications.SetItem(index, new InsAssemblyQualification(name, value));
		}

		return qualifications;
	}

	static ImmutableArray<InsAssemblyQualification> WithoutQualification(this ImmutableArray<InsAssemblyQualification> qualifications, string name)
	{
		var index = qualifications.IndexOfQualification(name);

		return index < 0
			? qualifications
			: qualifications.RemoveAt(index);
	}

	static int IndexOfQualification(this ImmutableArray<InsAssemblyQualification> qualifications, string name)
	{
		for (var i = 0; i < qualifications.Length; i++)
		{
			if (qualifications[i].Name == name)
			{
				return i;
			}
		}

		return -1;
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
