// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
namespace TypeNameInterpretation;

/// <summary>
/// Visitor interface for visiting <see cref="InsType"/> syntax tree nodes.
/// </summary>
/// <typeparam name="TArgument">The type of the user-defined argument passed to visitor methods.</typeparam>
/// <typeparam name="TReturn">The return type of the visitor operations.</typeparam>
public interface IInsTypeVisitor<TArgument, TReturn>
{
	/// <summary>
	/// Visits an <see cref="InsArrayType"/> node representing a multi-dimensional or non-zero-bound array.
	/// </summary>
	/// <param name="type">The array type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitArray(InsArrayType type, TArgument argument);

	/// <summary>
	/// Visits an <see cref="InsByRefType"/> node representing a managed reference type.
	/// </summary>
	/// <param name="type">The by-ref type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitByRef(InsByRefType type, TArgument argument);

	/// <summary>
	/// Visits an <see cref="InsGenericType"/> node representing a constructed generic type.
	/// </summary>
	/// <param name="type">The generic type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitGeneric(InsGenericType type, TArgument argument);

	/// <summary>
	/// Visits an <see cref="InsNamedType"/> node representing a named type.
	/// </summary>
	/// <param name="type">The named type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitNamed(InsNamedType type, TArgument argument);

	/// <summary>
	/// Visits an <see cref="InsPointerType"/> node representing an unmanaged pointer type.
	/// </summary>
	/// <param name="type">The pointer type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitPointer(InsPointerType type, TArgument argument);

	/// <summary>
	/// Visits an <see cref="InsSZArrayType"/> node representing a single-dimensional vector array type.
	/// </summary>
	/// <param name="type">The single-dimensional array type node to visit.</param>
	/// <param name="argument">User-defined argument.</param>
	/// <returns>The result of the visit operation.</returns>
	TReturn VisitSZArray(InsSZArrayType type, TArgument argument);
}
