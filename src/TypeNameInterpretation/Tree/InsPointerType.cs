// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Represents an unmanaged pointer type (for example, <c>int*</c>).
/// </summary>
public sealed class InsPointerType : InsElementedType
{
	internal InsPointerType(InsType elementType)
		: base(elementType)
	{
	}

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.Pointer;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitPointer(this, argument);
	}
}
