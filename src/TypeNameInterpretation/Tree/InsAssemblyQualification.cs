// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation
{
	public sealed class InsAssemblyQualification
	{
		internal InsAssemblyQualification(string name, string value)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Value = value ?? throw new ArgumentNullException(nameof(value));
		}

		public string Name { get; }
		public string Value { get; }
	}
}
