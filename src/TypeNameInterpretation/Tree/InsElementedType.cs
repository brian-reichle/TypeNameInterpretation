// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation;

/// <summary>
/// Abstract base class for <see cref="InsType"/> nodes that encapsulate an underlying element type (e.g. arrays, pointers, by-ref types).
/// </summary>
public abstract class InsElementedType : InsType
{
	private protected InsElementedType(InsType elementType)
	{
		ArgumentNullException.ThrowIfNull(elementType);
		ElementType = elementType;
	}

	/// <summary>
	/// Gets the underlying element type.
	/// </summary>
	public InsType ElementType { get; }
}
