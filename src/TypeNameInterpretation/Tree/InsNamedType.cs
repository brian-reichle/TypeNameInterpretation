// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Diagnostics.CodeAnalysis;

namespace TypeNameInterpretation;

/// <summary>
/// Represents a named type (for example, <c>System.String</c>), which may optionally be nested or qualified with an assembly reference.
/// </summary>
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

	/// <summary>
	/// Gets the full type name.
	/// </summary>
	public string Name { get; }

	/// <summary>
	/// Gets the declaring outer type if this is a nested type; otherwise, <see langword="null"/>.
	/// </summary>
	public InsNamedType? DeclaringType { get; }

	/// <summary>
	/// Gets the qualifying assembly reference if specified on this top-level type; otherwise, <see langword="null"/>.
	/// </summary>
	public InsAssembly? Assembly { get; }

	/// <inheritdoc />
	public override InsTypeKind Kind => InsTypeKind.Named;

	/// <inheritdoc />
	public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(visitor);
		return visitor.VisitNamed(this, argument);
	}

	internal override bool TryFastFormat([NotNullWhen(true)] out string? value)
	{
		if (Assembly == null && DeclaringType == null && !Name.AsSpan().ContainsAny(Delimiters.All))
		{
			value = Name;
			return true;
		}
		else
		{
			value = null;
			return false;
		}
	}
}
