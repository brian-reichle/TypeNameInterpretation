// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation;

/// <summary>
/// Abstract base visitor for transforming or rewriting an <see cref="InsType"/> syntax tree.
/// </summary>
/// <typeparam name="TArgument">The type of user-defined argument passed through visitor methods.</typeparam>
/// <remarks>
/// Re-uses existing tree nodes when no modifications are made to child elements (structural sharing).
/// </remarks>
public abstract class InsRewriter<TArgument> : IInsTypeVisitor<TArgument, InsType>
{
	/// <summary>
	/// Rewrites an <see cref="InsArrayType"/> node.
	/// </summary>
	/// <param name="type">The array type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitArray(InsArrayType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		var elementType = type.ElementType.Apply(this, argument);

		if (elementType == type.ElementType)
		{
			return type;
		}

		return new InsArrayType(elementType, type.Rank);
	}

	/// <summary>
	/// Rewrites an <see cref="InsByRefType"/> node.
	/// </summary>
	/// <param name="type">The by-ref type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitByRef(InsByRefType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		var inner = type.ElementType.Apply(this, argument);

		if (inner == type.ElementType)
		{
			return type;
		}

		return new InsByRefType(inner);
	}

	/// <summary>
	/// Rewrites an <see cref="InsGenericType"/> node.
	/// </summary>
	/// <param name="type">The generic type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitGeneric(InsGenericType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		var definition = (InsNamedType)type.Definition.Apply(this, argument);
		var typeArguments = VisitTypes(type.TypeArguments, argument);

		if (definition == type.Definition && typeArguments == type.TypeArguments)
		{
			return type;
		}

		return new InsGenericType(definition, typeArguments);
	}

	/// <summary>
	/// Rewrites an <see cref="InsNamedType"/> node.
	/// </summary>
	/// <param name="type">The named type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitNamed(InsNamedType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		if (type.DeclaringType != null)
		{
			var declaringType = (InsNamedType)type.DeclaringType.Apply(this, argument);

			if (declaringType == type.DeclaringType)
			{
				return type;
			}

			return new InsNamedType(type.Name, declaringType);
		}
		else
		{
			var assembly = type.Assembly == null ? null : VisitAssembly(type.Assembly, argument);

			if (assembly == type.Assembly)
			{
				return type;
			}

			return new InsNamedType(type.Name, assembly);
		}
	}

	/// <summary>
	/// Rewrites an <see cref="InsPointerType"/> node.
	/// </summary>
	/// <param name="type">The pointer type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitPointer(InsPointerType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		var inner = type.ElementType.Apply(this, argument);

		if (inner == type.ElementType)
		{
			return type;
		}

		return new InsPointerType(inner);
	}

	/// <summary>
	/// Rewrites an <see cref="InsSZArrayType"/> node.
	/// </summary>
	/// <param name="type">The single-dimensional array type node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsType"/> node, or <paramref name="type"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public virtual InsType VisitSZArray(InsSZArrayType type, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(type);
		var elementType = type.ElementType.Apply(this, argument);

		if (elementType == type.ElementType)
		{
			return type;
		}

		return new InsSZArrayType(elementType);
	}

	/// <summary>
	/// Rewrites a collection of generic type arguments.
	/// </summary>
	/// <param name="typeArguments">The immutable array of type arguments to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>An immutable array of rewritten type arguments, or the original array if unchanged.</returns>
	public virtual ImmutableArray<InsType> VisitTypes(ImmutableArray<InsType> typeArguments, TArgument argument)
	{
		ImmutableArray<InsType>.Builder? builder = null;

		for (var i = 0; i < typeArguments.Length; i++)
		{
			var type = typeArguments[i].Apply(this, argument);

			if (builder != null)
			{
				builder[i] = type;
			}
			else if (type != typeArguments[i])
			{
				builder = ImmutableArray.CreateBuilder<InsType>(typeArguments.Length);
				builder.AddRange(typeArguments, i);
				builder.Count = typeArguments.Length;
				builder[i] = type;
			}
		}

		return builder == null ? typeArguments : builder.MoveToImmutable();
	}

	/// <summary>
	/// Rewrites an <see cref="InsAssembly"/> node.
	/// </summary>
	/// <param name="assembly">The assembly node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsAssembly"/> node, or <paramref name="assembly"/> if unchanged.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public virtual InsAssembly VisitAssembly(InsAssembly assembly, TArgument argument)
	{
		ArgumentNullException.ThrowIfNull(assembly);
		var qualifications = VisitAssemblyQualifications(assembly.Qualifications, argument);

		if (qualifications == assembly.Qualifications)
		{
			return assembly;
		}

		return new InsAssembly(assembly.Name, qualifications);
	}

	/// <summary>
	/// Rewrites a collection of assembly qualifications.
	/// </summary>
	/// <param name="qualifications">The immutable array of assembly qualifications to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>An immutable array of rewritten assembly qualifications, or the original array if unchanged.</returns>
	public virtual ImmutableArray<InsAssemblyQualification> VisitAssemblyQualifications(ImmutableArray<InsAssemblyQualification> qualifications, TArgument argument)
	{
		ImmutableArray<InsAssemblyQualification>.Builder? builder = null;

		for (var i = 0; i < qualifications.Length; i++)
		{
			var type = VisitAssemblyQualification(qualifications[i], argument);

			if (builder != null)
			{
				builder[i] = type;
			}
			else if (type != qualifications[i])
			{
				builder = ImmutableArray.CreateBuilder<InsAssemblyQualification>(qualifications.Length);
				builder.AddRange(qualifications, i);
				builder.Count = qualifications.Length;
				builder[i] = type;
			}
		}

		return builder == null ? qualifications : builder.MoveToImmutable();
	}

	/// <summary>
	/// Rewrites an <see cref="InsAssemblyQualification"/> node.
	/// </summary>
	/// <param name="qualification">The assembly qualification node to rewrite.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>A non-null rewritten <see cref="InsAssemblyQualification"/> node, or <paramref name="qualification"/> if unchanged.</returns>
	public virtual InsAssemblyQualification VisitAssemblyQualification(InsAssemblyQualification qualification, TArgument argument) => qualification;
}
