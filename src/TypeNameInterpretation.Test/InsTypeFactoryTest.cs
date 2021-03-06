using System.Linq;
using NUnit.Framework;
using static TypeNameInterpretation.InsTypeFactory;

namespace TypeNameInterpretation.Test
{
	[TestFixture]
	class InsTypeFactoryTest
	{
		[Test]
		public void AssemblyQualificationFactory()
		{
			var qualification = Qualification("name", "value");
			Assert.That(qualification.Name, Is.EqualTo("name"));
			Assert.That(qualification.Value, Is.EqualTo("value"));
		}

		[Test]
		public void AssemblyFactory()
		{
			var qualification = Qualification("A", "B");
			var assembly = Assembly("AssemblyName", qualification);

			Assert.That(assembly.Name, Is.EqualTo("AssemblyName"));
			Assert.That(assembly.Qualifications.Single(), Is.SameAs(qualification));
		}

		[Test]
		public void NamedTypeFactory()
		{
			var assembly = Assembly("A");
			var type1 = NamedType("B");
			var type2 = NamedType("TypeName", assembly);

			Assert.That(type1.Kind, Is.EqualTo(InsTypeKind.Named));
			Assert.That(type1.Name, Is.EqualTo("B"));
			Assert.That(type1.Assembly, Is.Null);
			Assert.That(type1.DeclaringType, Is.Null);

			Assert.That(type2.Kind, Is.EqualTo(InsTypeKind.Named));
			Assert.That(type2.Name, Is.EqualTo("TypeName"));
			Assert.That(type2.Assembly, Is.SameAs(assembly));
			Assert.That(type2.DeclaringType, Is.Null);
		}

		[Test]
		public void ArrayTypeFactory()
		{
			var elementType = NamedType("A");
			var type = ArrayType(elementType, 2);

			Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Array));
			Assert.That(type.ElementType, Is.SameAs(elementType));
			Assert.That(type.Rank, Is.EqualTo(2));
		}

		[Test]
		public void PointerTypeFactory()
		{
			var elementType = NamedType("A");
			var type = PointerType(elementType);

			Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Pointer));
			Assert.That(type.ElementType, Is.SameAs(elementType));
		}

		[Test]
		public void ByRefTypeFactory()
		{
			var elementType = NamedType("A");
			var type = ByRefType(elementType);

			Assert.That(type.Kind, Is.EqualTo(InsTypeKind.ByRef));
			Assert.That(type.ElementType, Is.SameAs(elementType));
		}

		[Test]
		public void NestedFactory()
		{
			var declaringType = NamedType("B");
			var type = NestedType(declaringType, "TypeName");

			Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Named));
			Assert.That(type.Name, Is.EqualTo("TypeName"));
			Assert.That(type.Assembly, Is.Null);
			Assert.That(type.DeclaringType, Is.EqualTo(declaringType));
		}

		[Test]
		public void GenericTypeFactory()
		{
			var type1 = NamedType("A");
			var type2 = NamedType("B");
			var generic = Generic(type1, type2);

			Assert.That(generic.Kind, Is.EqualTo(InsTypeKind.Generic));
			Assert.That(generic.Definition, Is.EqualTo(type1));
			Assert.That(generic.TypeArguments.Single(), Is.SameAs(type2));
		}

		[Test]
		public void SZArrayTypeFactory()
		{
			var elementType = NamedType("A");
			var type = SZArrayType(elementType);

			Assert.That(type.Kind, Is.EqualTo(InsTypeKind.SZArray));
			Assert.That(type.ElementType, Is.SameAs(elementType));
		}
	}
}
