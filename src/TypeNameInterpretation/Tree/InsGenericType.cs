// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a constructed generic type with type arguments (for example, <c>System.Collections.Generic.List`1[System.String]</c>).
/// </summary>
public sealed class InsGenericType : InsType
{
	internal InsGenericType(InsNamedType definition, ImmutableArray<InsType> typeArguments)
	{
		ArgumentNullException.ThrowIfNull(definition);
		Definition = definition;
		TypeArguments = typeArguments;
	}

	/// <summary>
	/// Gets the named generic type definition.
	/// </summary>
	public InsNamedType Definition { get; }

	/// <summary>
	/// Gets the collection of generic type arguments.
	/// </summary>
	public ImmutableArray<InsType> TypeArguments { get; }

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.Generic;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitGeneric(this, argument);
	}
}
