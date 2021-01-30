using System.Linq;
using NUnit.Framework;

namespace TypeInterpretation.Test
{
	[TestFixture]
	class InsParserTest
	{
		[TestCase("Foo.Bar", ExpectedResult = "Foo.Bar")]
		[TestCase("\\[Foo.Bar\\]", ExpectedResult = "[Foo.Bar]")]
		[TestCase("Foo\\\\Bar", ExpectedResult = "Foo\\Bar")]
		public string Assembly_UnqualifiedName(string assemblyName)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(assembly.Qualifications, Is.Empty);

			return assembly.Name;
		}

		[TestCase("Foo, Culture=\"\"", "Culture|")]
		[TestCase("Foo, Culture=neutral", "Culture|neutral")]
		[TestCase("Foo, Bar=Baz, Culture=neutral", "Bar|Baz", "Culture|neutral")]
		[TestCase("Foo, Bar=Baz", "Bar|Baz")]
		[TestCase("Foo, \"Bar\"=\"Baz\"", "Bar|Baz")]
		[TestCase("Foo, Bar=Baz\\,", "Bar|Baz,")]
		[TestCase("Foo, Bar=\"Baz,\"", "Bar|Baz,")]
		[TestCase("Foo, Bar=Ba\\\"z", "Bar|Ba\"z")]
		[TestCase("Foo, Bar=\"Ba\\\"z\"", "Bar|Ba\"z")]
		public void Assembly_QualifiedName(string assemblyName, params string[] qualifications)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(assembly.Name, Is.EqualTo("Foo"));
			Assert.That(assembly.Qualifications.Select(x => x.Name + "|" + x.Value), Is.EqualTo(qualifications));
		}

		[Test]
		public void Type_Unqualified()
		{
			var expected =
@"NamedType:
  ""Foo.Bar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Qualified()
		{
			var expected =
@"NamedType:
  ""Foo.Bar.Baz""
  Assembly:
    ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar.Baz, FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_FullyQualified()
		{
			var expected =
@"NamedType:
  ""Foo.Bar.Baz""
  Assembly:
    ""FooBar""
    Qualification:
      ""Culture""
      ""neutral""
    Qualification:
      ""Version""
      ""3.14""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar.Baz, FooBar, Culture=neutral, Version=3.14")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Nested()
		{
			var expected =
@"NamedType:
  ""Baz""
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar+Baz, FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_GenericWithUnquolifiedArg1()
		{
			var expected =
@"Generic:
  NamedType:
    ""Foo.Bar`1""
    Assembly:
      ""FooBar""
  NamedType:
    ""Baz""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`1[Baz], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_GenericWithUnquolifiedArg2()
		{
			var expected =
@"Generic:
  NamedType:
    ""Foo.Bar`2""
    Assembly:
      ""FooBar""
  NamedType:
    ""Baz""
  NamedType:
    ""Quux""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`2[Baz,Quux], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_GenericWithUnquolifiedArg3()
		{
			var expected =
@"Generic:
  NamedType:
    ""Foo.Bar`3""
    Assembly:
      ""FooBar""
  NamedType:
    ""Baz""
  NamedType:
    ""Quux""
  NamedType:
    ""Barry""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`3[Baz,Quux,Barry], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_GenericWithQuolifiedArg()
		{
			var expected =
@"Generic:
  NamedType:
    ""Foo.Bar`1""
    Assembly:
      ""FooBar""
  NamedType:
    ""Baz""
    Assembly:
      ""FooBaz""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`1[[Baz, FooBaz]], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_GenericOfArray()
		{
			var expected =
@"Generic:
  NamedType:
    ""Foo.Bar`1""
    Assembly:
      ""FooBar""
  SZArrayType:
    NamedType:
      ""Baz""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`1[Baz[]], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_ArrayOfGeneric()
		{
			var expected =
@"SZArrayType:
  Generic:
    NamedType:
      ""Foo.Bar`1""
      Assembly:
        ""FooBar""
    NamedType:
      ""Baz""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar`1[Baz][], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_SZArray()
		{
			var expected =
@"SZArrayType:
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar[], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Array1()
		{
			var expected =
@"ArrayType:
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
  1
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar[*], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Array2()
		{
			var expected =
@"ArrayType:
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
  2
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar[,], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Array2_1()
		{
			var expected =
@"ArrayType:
  ArrayType:
    NamedType:
      ""Foo.Bar""
      Assembly:
        ""FooBar""
    2
  1
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar[,][*], FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_Pointer()
		{
			var expected =
@"Pointer:
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar*, FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_PointerPointer()
		{
			var expected =
@"Pointer:
  Pointer:
    NamedType:
      ""Foo.Bar""
      Assembly:
        ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar**, FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_ByRef()
		{
			var expected =
@"ByRef:
  NamedType:
    ""Foo.Bar""
    Assembly:
      ""FooBar""
";

			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Foo.Bar&, FooBar")), Is.EqualTo(expected));
		}

		[Test]
		public void Type_ComplexAssemblyLocation()
		{
			var expected =
@"Generic:
  NamedType:
    ""Baz""
    Assembly:
      ""Quux""
  ArrayType:
    NamedType:
      ""Foo.Bar""
      Assembly:
        ""FooBar""
        Qualification:
          ""Culture""
          ""neu""tr]al""
        Qualification:
          ""Frob""
          ""bar]x""
        Qualification:
          ""Version""
          ""3.14""
    2
";

			// Ensure we can correctly locate the start of the assembly dispite complex syntax along the way.
			Assert.That(TreeRenderer.Format(InsTypeFactory.ParseTypeName("Baz[[Foo.Bar[,], FooBar, Culture=\"neu\\\"tr]al\", Frob=bar\\]x, Version=3.14]], Quux")), Is.EqualTo(expected));
		}
	}
}
