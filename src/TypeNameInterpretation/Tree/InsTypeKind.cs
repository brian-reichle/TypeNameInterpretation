// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
namespace TypeNameInterpretation;

/// <summary>
/// Specifies the kind of an <see cref="InsType"/> node.
/// </summary>
public enum InsTypeKind
{
	/// <summary>
	/// A multi-dimensional or non-zero-lower-bound array type (<see cref="InsArrayType"/>).
	/// </summary>
	Array,

	/// <summary>
	/// A managed reference type (<see cref="InsByRefType"/>).
	/// </summary>
	ByRef,

	/// <summary>
	/// A constructed generic type (<see cref="InsGenericType"/>).
	/// </summary>
	Generic,

	/// <summary>
	/// A named type (<see cref="InsNamedType"/>).
	/// </summary>
	Named,

	/// <summary>
	/// An unmanaged pointer type (<see cref="InsPointerType"/>).
	/// </summary>
	Pointer,

	/// <summary>
	/// A single-dimensional vector array type (<see cref="InsSZArrayType"/>).
	/// </summary>
	SZArray,
}
