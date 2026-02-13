// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
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
			using (Assert.EnterMultipleScope())
			{
				Assert.That(qualification.Name, Is.EqualTo("name"), nameof(qualification.Name));
				Assert.That(qualification.Value, Is.EqualTo("value"), nameof(qualification.Value));
			}
		}

		[Test]
		public void AssemblyFactory()
		{
			var qualification = Qualification("A", "B");
			var assembly = Assembly("AssemblyName", qualification);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(assembly.Name, Is.EqualTo("AssemblyName"), nameof(assembly.Name));
				Assert.That(assembly.Qualifications.Single(), Is.SameAs(qualification), nameof(assembly.Qualifications));
			}
		}

		[Test]
		public void NamedTypeFactory()
		{
			var assembly = Assembly("A");
			var type1 = NamedType("B");
			var type2 = NamedType("TypeName", assembly);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type1.Kind, Is.EqualTo(InsTypeKind.Named), nameof(type1.Kind) + " 1");
				Assert.That(type1.Name, Is.EqualTo("B"), nameof(type1.Name) + " 1");
				Assert.That(type1.Assembly, Is.Null, nameof(type1.Assembly) + " 1");
				Assert.That(type1.DeclaringType, Is.Null, nameof(type1.DeclaringType) + " 1");

				Assert.That(type2.Kind, Is.EqualTo(InsTypeKind.Named), nameof(type2.Kind) + " 2");
				Assert.That(type2.Name, Is.EqualTo("TypeName"), nameof(type2.Name) + " 2");
				Assert.That(type2.Assembly, Is.SameAs(assembly), nameof(type2.Assembly) + " 2");
				Assert.That(type2.DeclaringType, Is.Null, nameof(type2.DeclaringType) + " 2");
			}
		}

		[Test]
		public void ArrayTypeFactory()
		{
			var elementType = NamedType("A");
			var type = ArrayType(elementType, 2);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Array), nameof(type.Kind));
				Assert.That(type.ElementType, Is.SameAs(elementType), nameof(type.ElementType));
				Assert.That(type.Rank, Is.EqualTo(2), nameof(type.Rank));
			}
		}

		[Test]
		public void PointerTypeFactory()
		{
			var elementType = NamedType("A");
			var type = PointerType(elementType);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Pointer), nameof(type.Kind));
				Assert.That(type.ElementType, Is.SameAs(elementType), nameof(type.ElementType));
			}
		}

		[Test]
		public void ByRefTypeFactory()
		{
			var elementType = NamedType("A");
			var type = ByRefType(elementType);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type.Kind, Is.EqualTo(InsTypeKind.ByRef), nameof(type.Kind));
				Assert.That(type.ElementType, Is.SameAs(elementType), nameof(type.ElementType));
			}
		}

		[Test]
		public void NestedFactory()
		{
			var declaringType = NamedType("B");
			var type = NestedType(declaringType, "TypeName");

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type.Kind, Is.EqualTo(InsTypeKind.Named), nameof(type.Kind));
				Assert.That(type.Name, Is.EqualTo("TypeName"), nameof(type.Name));
				Assert.That(type.Assembly, Is.Null, nameof(type.Assembly));
				Assert.That(type.DeclaringType, Is.EqualTo(declaringType), nameof(type.DeclaringType));
			}
		}

		[Test]
		public void GenericTypeFactory()
		{
			var type1 = NamedType("A");
			var type2 = NamedType("B");
			var generic = Generic(type1, type2);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(generic.Kind, Is.EqualTo(InsTypeKind.Generic), nameof(generic.Kind));
				Assert.That(generic.Definition, Is.EqualTo(type1), nameof(generic.Definition));
				Assert.That(generic.TypeArguments.Single(), Is.SameAs(type2), nameof(generic.TypeArguments));
			}
		}

		[Test]
		public void SZArrayTypeFactory()
		{
			var elementType = NamedType("A");
			var type = SZArrayType(elementType);

			using (Assert.EnterMultipleScope())
			{
				Assert.That(type.Kind, Is.EqualTo(InsTypeKind.SZArray), nameof(type.Kind));
				Assert.That(type.ElementType, Is.SameAs(elementType), nameof(type.ElementType));
			}
		}
	}
}
