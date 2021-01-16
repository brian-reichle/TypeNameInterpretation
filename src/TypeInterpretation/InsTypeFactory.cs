using System.Collections.Immutable;

namespace TypeInterpretation
{
	public static class InsTypeFactory
	{
		public static InsArrayType ArrayType(InsType elementType, int array)
			=> new InsArrayType(elementType, array);

		public static InsNamedType NamedType(string name, InsAssembly? assembly = null, params InsType[] typeArguments)
			=> NamedType(name, assembly, ImmutableArray.Create(typeArguments));

		public static InsNamedType NamedType(string name, InsAssembly? assembly, ImmutableArray<InsType> typeArguments)
			=> new InsNamedType(name, assembly, typeArguments);

		public static InsPointerType PointerType(InsType elementType)
			=> new InsPointerType(elementType);

		public static InsByRefType ByRefType(InsType elementType)
			=> new InsByRefType(elementType);

		public static InsAssembly Assembly(string name, params InsAssemblyQualification[] qualifications)
			=> new InsAssembly(name, ImmutableArray.Create(qualifications));

		public static InsAssembly Assembly(string name, ImmutableArray<InsAssemblyQualification> qualifications)
			=> new InsAssembly(name, qualifications);

		public static InsAssemblyQualification Qualification(string name, string value)
			=> new InsAssemblyQualification(name, value);
	}
}
