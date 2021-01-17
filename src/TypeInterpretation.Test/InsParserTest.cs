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
		[TestCase("Foo, Bar=Ba\"z", "Bar|Ba\"z")]
		[TestCase("Foo, Bar=\"Ba\\\"z\"", "Bar|Ba\"z")]
		public void Assembly_QualifiedName(string assemblyName, params string[] qualifications)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(assembly.Name, Is.EqualTo("Foo"));
			Assert.That(assembly.Qualifications.Select(x => x.Name + "|" + x.Value), Is.EqualTo(qualifications));
		}
	}
}
