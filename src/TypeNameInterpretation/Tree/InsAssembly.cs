// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation;

public sealed class InsAssembly
{
	internal InsAssembly(string name, ImmutableArray<InsAssemblyQualification> qualifications)
	{
		ArgumentNullException.ThrowIfNull(name);
		Name = name;
		Qualifications = qualifications;
	}

	public string Name { get; }
	public ImmutableArray<InsAssemblyQualification> Qualifications { get; }
	public override string ToString() => InsFormatter.Format(this);
}
