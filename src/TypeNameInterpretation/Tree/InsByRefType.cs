// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a managed reference type (for example, <c>int&amp;</c>).
/// </summary>
public sealed class InsByRefType : InsElementedType
{
	internal InsByRefType(InsType elementType)
		: base(elementType)
	{
	}

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.ByRef;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitByRef(this, argument);
	}
}
