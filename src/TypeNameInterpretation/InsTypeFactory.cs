// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation;

/// <summary>
/// Factory and utility methods for creating and parsing <see cref="InsType"/> and <see cref="InsAssembly"/> instances.
/// </summary>
public static class InsTypeFactory
{
	/// <summary>
	/// Creates an <see cref="InsArrayType"/> representing a multi-dimensional or non-zero-lower-bound array.
	/// </summary>
	/// <param name="elementType">The type of elements in the array.</param>
	/// <param name="rank">The number of dimensions of the array.</param>
	/// <returns>A new <see cref="InsArrayType"/> instance with the specified element type and rank.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="elementType"/> is <see langword="null"/>.</exception>
	public static InsArrayType ArrayType(InsType elementType, int rank) => new(elementType, rank);

	/// <summary>
	/// Creates an <see cref="InsByRefType"/> representing a managed reference type.
	/// </summary>
	/// <param name="elementType">The referenced target type.</param>
	/// <returns>A new <see cref="InsByRefType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="elementType"/> is <see langword="null"/>.</exception>
	public static InsByRefType ByRefType(InsType elementType) => new(elementType);

	/// <summary>
	/// Creates an <see cref="InsGenericType"/> representing a constructed generic type with type arguments.
	/// </summary>
	/// <param name="definition">The generic type definition.</param>
	/// <param name="typeArguments">The generic type arguments.</param>
	/// <returns>A new <see cref="InsGenericType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="definition"/> or <paramref name="typeArguments"/> is <see langword="null"/>.</exception>
	public static InsGenericType Generic(InsNamedType definition, params InsType[] typeArguments) => Generic(definition, ImmutableArray.Create(typeArguments));

	/// <summary>
	/// Creates an <see cref="InsGenericType"/> representing a constructed generic type with type arguments.
	/// </summary>
	/// <param name="definition">The generic type definition.</param>
	/// <param name="typeArguments">The immutable array of generic type arguments.</param>
	/// <returns>A new <see cref="InsGenericType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="definition"/> is <see langword="null"/>.</exception>
	public static InsGenericType Generic(InsNamedType definition, ImmutableArray<InsType> typeArguments) => new(definition, typeArguments);

	/// <summary>
	/// Creates an <see cref="InsNamedType"/> representing a non-nested named type with an optional assembly reference.
	/// </summary>
	/// <param name="name">The name of the type.</param>
	/// <param name="assembly">The optional assembly reference, or <see langword="null"/> if unqualified.</param>
	/// <returns>A new <see cref="InsNamedType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
	public static InsNamedType NamedType(string name, InsAssembly? assembly = null) => new(name, assembly);

	/// <summary>
	/// Creates an <see cref="InsNamedType"/> representing a nested named type.
	/// </summary>
	/// <param name="declaringType">The declaring outer type.</param>
	/// <param name="name">The nested type name.</param>
	/// <returns>A new <see cref="InsNamedType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="declaringType"/> or <paramref name="name"/> is <see langword="null"/>.</exception>
	public static InsNamedType NestedType(InsNamedType declaringType, string name) => new(name, declaringType);

	/// <summary>
	/// Creates an <see cref="InsPointerType"/> representing an unmanaged pointer type.
	/// </summary>
	/// <param name="elementType">The pointed-to element type.</param>
	/// <returns>A new <see cref="InsPointerType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="elementType"/> is <see langword="null"/>.</exception>
	public static InsPointerType PointerType(InsType elementType) => new(elementType);

	/// <summary>
	/// Creates an <see cref="InsSZArrayType"/> representing a single-dimensional vector array type.
	/// </summary>
	/// <param name="elementType">The element type of the array.</param>
	/// <returns>A new <see cref="InsSZArrayType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="elementType"/> is <see langword="null"/>.</exception>
	public static InsSZArrayType SZArrayType(InsType elementType) => new(elementType);

	/// <summary>
	/// Creates an <see cref="InsAssembly"/> reference with qualifications.
	/// </summary>
	/// <param name="name">The simple name of the assembly.</param>
	/// <param name="qualifications">The assembly qualifications.</param>
	/// <returns>A new <see cref="InsAssembly"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="qualifications"/> is <see langword="null"/>.</exception>
	public static InsAssembly Assembly(string name, params InsAssemblyQualification[] qualifications) => new(name, ImmutableArray.Create(qualifications));

	/// <summary>
	/// Creates an <see cref="InsAssembly"/> reference with qualifications.
	/// </summary>
	/// <param name="name">The simple name of the assembly.</param>
	/// <param name="qualifications">The immutable array of assembly qualifications.</param>
	/// <returns>A new <see cref="InsAssembly"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> is <see langword="null"/>.</exception>
	public static InsAssembly Assembly(string name, ImmutableArray<InsAssemblyQualification> qualifications) => new(name, qualifications);

	/// <summary>
	/// Creates an <see cref="InsAssemblyQualification"/> key-value pair.
	/// </summary>
	/// <param name="name">The qualification key name.</param>
	/// <param name="value">The qualification value.</param>
	/// <returns>A new <see cref="InsAssemblyQualification"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="name"/> or <paramref name="value"/> is <see langword="null"/>.</exception>
	public static InsAssemblyQualification Qualification(string name, string value) => new(name, value);

	/// <summary>
	/// Parses an assembly reference string representation into an <see cref="InsAssembly"/>.
	/// </summary>
	/// <param name="value">The assembly specification string to parse.</param>
	/// <returns>A non-null parsed <see cref="InsAssembly"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidTypeNameException">The specified string is not a valid assembly reference.</exception>
	public static InsAssembly ParseAssemblyName(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return InsParser.ParseAssembly(value.AsSpan());
	}

	/// <summary>
	/// Parses an assembly reference character span representation into an <see cref="InsAssembly"/>.
	/// </summary>
	/// <param name="value">The assembly specification character span to parse.</param>
	/// <returns>A non-null parsed <see cref="InsAssembly"/> instance.</returns>
	/// <exception cref="InvalidTypeNameException">The specified character span is not a valid assembly reference.</exception>
	public static InsAssembly ParseAssemblyName(ReadOnlySpan<char> value) => InsParser.ParseAssembly(value);

	/// <summary>
	/// Parses a type name string representation into an <see cref="InsType"/> syntax tree.
	/// </summary>
	/// <param name="value">The type name string to parse.</param>
	/// <returns>A non-null parsed <see cref="InsType"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="value"/> is <see langword="null"/>.</exception>
	/// <exception cref="InvalidTypeNameException">The specified string is not a valid type name.</exception>
	public static InsType ParseTypeName(string value)
	{
		ArgumentNullException.ThrowIfNull(value);
		return InsParser.ParseType(value.AsSpan());
	}

	/// <summary>
	/// Parses a type name character span representation into an <see cref="InsType"/> syntax tree.
	/// </summary>
	/// <param name="value">The type name character span to parse.</param>
	/// <returns>A non-null parsed <see cref="InsType"/> instance.</returns>
	/// <exception cref="InvalidTypeNameException">The specified character span is not a valid type name.</exception>
	public static InsType ParseTypeName(ReadOnlySpan<char> value) => InsParser.ParseType(value);
}
