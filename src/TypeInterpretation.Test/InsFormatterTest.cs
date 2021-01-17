using NUnit.Framework;
using static TypeInterpretation.InsFormatter;
using static TypeInterpretation.InsTypeFactory;

namespace TypeInterpretation.Test
{
	[TestFixture]
	class InsFormatterTest
	{
		[TestCase("Foo", ExpectedResult = "Foo")]
		[TestCase("[Foo]", ExpectedResult = "\\[Foo\\]")]
		[TestCase("Foo&", ExpectedResult = "Foo\\&")]
		[TestCase("Foo*", ExpectedResult = "Foo\\*")]
		[TestCase("Foo+", ExpectedResult = "Foo\\+")]
		[TestCase("Foo\\Baz", ExpectedResult = "Foo\\\\Baz")]
		public string SimpleNamedType(string identifierName)
		{
			return Format(NamedType(identifierName));
		}

		[TestCase("Bar", ExpectedResult = "Foo, Bar")]
		[TestCase("[Bar]", ExpectedResult = "Foo, \\[Bar\\]")]
		[TestCase("Bar&", ExpectedResult = "Foo, Bar\\&")]
		[TestCase("Bar*", ExpectedResult = "Foo, Bar\\*")]
		[TestCase("Bar+", ExpectedResult = "Foo, Bar\\+")]
		[TestCase("Bar\\Baz", ExpectedResult = "Foo, Bar\\\\Baz")]
		public string AssemblyQualifiedNamedType(string identifier)
		{
			return Format(NamedType("Foo", Assembly(identifier)));
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

		[TestCase(true, ExpectedResult = "Foo+Baz, Bar")]
		[TestCase(false, ExpectedResult = "Foo+Baz")]
		public string Nested(bool qualified)
		{
			return Format(NestedType(NamedType("Foo", qualified ? _unqualifiedAssembly : null), "Baz"));
		}

		[TestCase(1, false, ExpectedResult = "Foo[]")]
		[TestCase(2, false, ExpectedResult = "Foo[,]")]
		[TestCase(1, true, ExpectedResult = "Foo[], Bar")]
		[TestCase(2, true, ExpectedResult = "Foo[,], Bar")]
		public string Array(int rank, bool qualified)
		{
			return Format(ArrayType(NamedType("Foo", qualified ? _unqualifiedAssembly : null), rank));
		}

		[Test]
		public void ArrayOfArrays()
		{
			Assert.That(Format(ArrayType(ArrayType(NamedType("Foo"), 1), 2)), Is.EqualTo("Foo[][,]"));
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
