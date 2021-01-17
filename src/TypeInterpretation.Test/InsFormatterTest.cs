using NUnit.Framework;
using static TypeInterpretation.InsFormatter;
using static TypeInterpretation.InsTypeFactory;

namespace TypeInterpretation.Test
{
	[TestFixture]
	class InsFormatterTest
	{
		[Test]
		public void SimpleNamedType()
		{
			Assert.That(Format(NamedType("Foo")), Is.EqualTo("Foo"));
		}

		[Test]
		public void AssemblyQualifiedNamedType()
		{
			Assert.That(Format(NamedType("Foo", _unqualifiedAssembly)), Is.EqualTo("Foo, Bar"));
		}

		[Test]
		public void FullyQualifiedNamedType()
		{
			Assert.That(Format(NamedType("Foo", _qualifiedAssembly)), Is.EqualTo("Foo, Bar, Culture=neutral"));
		}

		[TestCase(true, true, ExpectedResult = "Foo`2[[Baz, Bar],[Quux, Bar, Culture=neutral]], Bar")]
		[TestCase(true, false, ExpectedResult = "Foo`2[[Baz],[Quux]], Bar")]
		[TestCase(false, true, ExpectedResult = "Foo`2[[Baz, Bar],[Quux, Bar, Culture=neutral]]")]
		[TestCase(false, false, ExpectedResult = "Foo`2[[Baz],[Quux]]")]
		public string GenericNamedType(bool qualifiedOuter, bool qualifiedInner)
		{
			var type = NamedType(
				"Foo`2",
				qualifiedOuter ? _unqualifiedAssembly : null,
				NamedType(
					"Baz",
					qualifiedInner ? _unqualifiedAssembly : null),
				NamedType(
					"Quux",
					qualifiedInner ? _qualifiedAssembly : null));

			return Format(type);
		}

		[TestCase(1, false, ExpectedResult = "Foo[]")]
		[TestCase(2, false, ExpectedResult = "Foo[,]")]
		[TestCase(1, true, ExpectedResult = "Foo[], Bar")]
		[TestCase(2, true, ExpectedResult = "Foo[,], Bar")]
		public string Array(int rank, bool qualified)
		{
			return Format(ArrayType(NamedType("Foo", qualified ? _unqualifiedAssembly : null), rank));
		}

		[TestCase(false, ExpectedResult = "Foo&")]
		[TestCase(true, ExpectedResult = "Foo&, Bar")]
		public string ByRef(bool qualified)
		{
			return Format(ByRefType(NamedType("Foo", qualified ? _unqualifiedAssembly : null)));
		}

		[TestCase(false, ExpectedResult = "Foo*")]
		[TestCase(true, ExpectedResult = "Foo*, Bar")]
		public string Pointer(bool qualified)
		{
			return Format(PointerType(NamedType("Foo", qualified ? _unqualifiedAssembly : null)));
		}

		[TestCase(true, ExpectedResult = "Bar, Culture=neutral")]
		[TestCase(false, ExpectedResult = "Bar")]
		public string FormatAssembly(bool qualified)
		{
			return Format(qualified ? _qualifiedAssembly : _unqualifiedAssembly);
		}

		readonly InsAssembly _unqualifiedAssembly = Assembly("Bar");
		readonly InsAssembly _qualifiedAssembly = Assembly("Bar", Qualification("Culture", "neutral"));
	}
}
