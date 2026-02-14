// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

public sealed class InsAssemblyQualification
{
	internal InsAssemblyQualification(string name, string value)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(value);
		Name = name;
		Value = value;
	}

	public string Name { get; }
	public string Value { get; }
}
