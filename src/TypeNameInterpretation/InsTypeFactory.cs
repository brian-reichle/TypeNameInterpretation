// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation
{
	public static class InsTypeFactory
	{
		public static InsArrayType ArrayType(InsType elementType, int rank) => new(elementType, rank);
		public static InsByRefType ByRefType(InsType elementType) => new(elementType);
		public static InsGenericType Generic(InsNamedType definition, params InsType[] typeArguments) => Generic(definition, ImmutableArray.Create(typeArguments));
		public static InsGenericType Generic(InsNamedType definition, ImmutableArray<InsType> typeArguments) => new(definition, typeArguments);
		public static InsNamedType NamedType(string name, InsAssembly? assembly = null) => new(name, assembly);
		public static InsNamedType NestedType(InsNamedType declaringType, string name) => new(name, declaringType);
		public static InsPointerType PointerType(InsType elementType) => new(elementType);
		public static InsSZArrayType SZArrayType(InsType elementType) => new(elementType);

		public static InsAssembly Assembly(string name, params InsAssemblyQualification[] qualifications) => new(name, ImmutableArray.Create(qualifications));
		public static InsAssembly Assembly(string name, ImmutableArray<InsAssemblyQualification> qualifications) => new(name, qualifications);

		public static InsAssemblyQualification Qualification(string name, string value) => new(name, value);

		public static InsAssembly ParseAssemblyName(string value) => InsParser.ParseAssembly(value.AsSpan());
		public static InsAssembly ParseAssemblyName(ReadOnlySpan<char> value) => InsParser.ParseAssembly(value);

		public static InsType ParseTypeName(string value) => InsParser.ParseType(value.AsSpan());
		public static InsType ParseTypeName(ReadOnlySpan<char> value) => InsParser.ParseType(value);
	}
}
