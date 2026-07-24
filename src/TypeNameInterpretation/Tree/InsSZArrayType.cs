// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a single-dimensional vector array type with zero lower bound (for example, <c>int[]</c>).
/// </summary>
public sealed class InsSZArrayType : InsElementedType
{
	internal InsSZArrayType(InsType elementType)
		: base(elementType)
	{
	}

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.SZArray;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitSZArray(this, argument);
	}
}
