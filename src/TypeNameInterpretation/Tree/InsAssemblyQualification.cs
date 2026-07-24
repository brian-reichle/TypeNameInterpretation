// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a key-value qualification pair on an assembly reference (for example, Version or PublicKeyToken).
/// </summary>
/// <remarks>
/// Instances of <see cref="InsAssemblyQualification"/> are immutable and thread-safe.
/// </remarks>
public sealed class InsAssemblyQualification
{
	internal InsAssemblyQualification(string name, string value)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(value);
		Name = name;
		Value = value;
	}

	/// <summary>
	/// Gets the qualification key name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the qualification value.
	/// </summary>
	public string Value { get; }
}
