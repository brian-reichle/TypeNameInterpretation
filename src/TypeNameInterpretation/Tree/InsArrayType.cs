// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a multi-dimensional or non-zero-bound array type (for example, <c>int[,]</c> or <c>int[*]</c>).
/// </summary>
public sealed class InsArrayType : InsElementedType
{
	internal InsArrayType(InsType elementType, int rank)
		: base(elementType)
	{
		Rank = rank;
	}

	/// <summary>
	/// Gets the number of dimensions (rank) of the array.
	/// </summary>
	public int Rank { get; }

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.Array;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitArray(this, argument);
	}
}
