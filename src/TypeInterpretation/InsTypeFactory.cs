using System;
using System.Collections.Immutable;

namespace TypeInterpretation
{
	public static class InsTypeFactory
	{
		public static InsArrayType ArrayType(InsType elementType, int array)
			=> new InsArrayType(elementType, array);

		public static InsNamedType NamedType(string name, InsAssembly? assembly = null)
			=> new InsNamedType(name, assembly);

		public static InsPointerType PointerType(InsType elementType)
			=> new InsPointerType(elementType);

		public static InsByRefType ByRefType(InsType elementType)
			=> new InsByRefType(elementType);

		public static InsNamedType NestedType(InsNamedType declaringType, string name)
			=> new InsNamedType(name, declaringType);

		public static InsGenericType Generic(InsNamedType definition, params InsType[] typeArguments)
			=> Generic(definition, ImmutableArray.Create(typeArguments));

		public static InsGenericType Generic(InsNamedType definition, ImmutableArray<InsType> typeArguments)
			=> new InsGenericType(definition, typeArguments);

		public static InsAssembly Assembly(string name, params InsAssemblyQualification[] qualifications)
			=> new InsAssembly(name, ImmutableArray.Create(qualifications));

		public static InsAssembly Assembly(string name, ImmutableArray<InsAssemblyQualification> qualifications)
			=> new InsAssembly(name, qualifications);

		public static InsAssemblyQualification Qualification(string name, string value)
			=> new InsAssemblyQualification(name, value);

		public static InsAssembly ParseAssemblyName(string value) => InsParser.ParseAssembly(value.AsSpan());
		public static InsAssembly ParseAssemblyName(ReadOnlySpan<char> value) => InsParser.ParseAssembly(value);
	}
}
