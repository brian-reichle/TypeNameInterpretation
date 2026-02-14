// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

public sealed class InsNamedType : InsType
{
	internal InsNamedType(
		string name,
		InsAssembly? assembly)
	{
		ArgumentNullException.ThrowIfNull(name);
		Name = name;
		Assembly = assembly;
	}

	internal InsNamedType(
		string name,
		InsNamedType? declaringType)
	{
		ArgumentNullException.ThrowIfNull(name);
		ArgumentNullException.ThrowIfNull(declaringType);
		Name = name;
		DeclaringType = declaringType;
	}

	public string Name { get; }
	public InsNamedType? DeclaringType { get; }
	public InsAssembly? Assembly { get; }
	public override InsTypeKind Kind => InsTypeKind.Named;

	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitNamed(this, argument);
	}
}
