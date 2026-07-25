// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace TypeNameInterpretation;

/// <summary>
/// Abstract base class for nodes in an interpreted .NET type name syntax tree.
/// </summary>
/// <remarks>
/// All subclasses of <see cref="InsType"/> are immutable and thread-safe.
/// </remarks>
public abstract class InsType
{
	private protected InsType()
	{
	}

	/// <summary>
	/// Gets the kind of type node.
	/// </summary>
	/// <remarks>
	/// This allows the consumer to switch on the kind rather than
	/// using repeated <see langword="as"/> casts.
	/// </remarks>
	public abstract InsTypeKind Kind { get; }

	/// <summary>
	/// Returns the canonical string representation of this type.
	/// </summary>
	/// <returns>A non-null formatted type name string.</returns>
	public sealed override string ToString() => InsFormatter.Format(this);

	/// <summary>
	/// Accepts a visitor to perform operations on this type node.
	/// </summary>
	/// <typeparam name="TArgument">The type of the user-defined argument passed to the visitor.</typeparam>
	/// <typeparam name="TReturn">The return type of the visitor operation.</typeparam>
	/// <param name="visitor">The visitor processing this node.</param>
	/// <param name="argument">User-defined argument passed to the visitor.</param>
	/// <returns>The result returned by the visitor.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="visitor"/> is <see langword="null"/>.</exception>
	public abstract TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument);

	internal virtual bool TryFastFormat([NotNullWhen(true)] out string? value)
	{
		value = null;
		return false;
	}
}
