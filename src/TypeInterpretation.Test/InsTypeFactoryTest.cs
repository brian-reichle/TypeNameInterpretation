using System.Linq;
using NUnit.Framework;
using static TypeInterpretation.InsTypeFactory;

namespace TypeInterpretation.Test
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
			var type2 = NamedType("TypeName", assembly, type1);

			Assert.That(type1.Kind, Is.EqualTo(InsTypeKind.Named));
			Assert.That(type1.Name, Is.EqualTo("B"));
			Assert.That(type1.Assembly, Is.Null);
			Assert.That(type1.DeclaringType, Is.Null);
			Assert.That(type1.TypeArguments, Is.Empty);

			Assert.That(type2.Kind, Is.EqualTo(InsTypeKind.Named));
			Assert.That(type2.Name, Is.EqualTo("TypeName"));
			Assert.That(type2.Assembly, Is.SameAs(assembly));
			Assert.That(type2.DeclaringType, Is.Null);
			Assert.That(type2.TypeArguments.Single(), Is.SameAs(type1));
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
	}
}
