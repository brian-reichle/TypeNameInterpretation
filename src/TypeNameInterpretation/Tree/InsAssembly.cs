// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation;

/// <summary>
/// Represents an assembly reference containing a simple assembly name and optional qualifications.
/// </summary>
/// <remarks>
/// Instances of <see cref="InsAssembly"/> are immutable and thread-safe.
/// </remarks>
public sealed class InsAssembly
{
	internal InsAssembly(string name, ImmutableArray<InsAssemblyQualification> qualifications)
	{
		ArgumentNullException.ThrowIfNull(name);
		Name = name;
		Qualifications = qualifications;
	}

	/// <summary>
	/// Gets the simple name of the assembly.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the collection of qualifications associated with this assembly reference.
	/// </summary>
	public ImmutableArray<InsAssemblyQualification> Qualifications { get; }

	/// <summary>
	/// Returns the formatted string representation of this assembly reference.
	/// </summary>
	/// <returns>A non-null formatted assembly reference string.</returns>
	public override string ToString() => InsFormatter.Format(this);
}
