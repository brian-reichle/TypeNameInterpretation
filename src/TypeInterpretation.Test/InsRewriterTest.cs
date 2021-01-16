using System;
using NUnit.Framework;
using static TypeInterpretation.InsTypeFactory;

namespace TypeInterpretation.Test
{
	[TestFixture]
	class InsRewriterTest
	{
		[Test]
		public void Unchanged()
		{
			var result = _allTheTypes.Apply(DummyRewriter.Instance, x => x);

			Assert.That(result, Is.SameAs(_allTheTypes));
		}

		[Test]
		public void RewriteAssemblyName()
		{
			var result = _allTheTypes.Apply(DummyRewriter.Instance, x => Replace(x, "Foo", "Bar"));

			const string ExpectedDiffs =
@"ByRef:
  Pointer:
    ArrayType:
      NamedType:
        ""TypeName""
        Assembly:
<<<<<<<
          ""Foo""
=======
          ""Bar""
>>>>>>>
          Qualification:
            ""Culture""
            ""neutral""
        NamedType:
          ""TArg""
      1
";

			Assert.That(TreeDiff.Diff(_allTheTypes, result), Is.EqualTo(ExpectedDiffs));
		}

		[Test]
		public void RewriteTypeName()
		{
			var result = _allTheTypes.Apply(DummyRewriter.Instance, x => Replace(x, "TypeName", "AnotherName"));

			const string ExpectedDiffs =
@"ByRef:
  Pointer:
    ArrayType:
      NamedType:
<<<<<<<
        ""TypeName""
=======
        ""AnotherName""
>>>>>>>
        Assembly:
          ""Foo""
          Qualification:
            ""Culture""
            ""neutral""
        NamedType:
          ""TArg""
      1
";

			Assert.That(TreeDiff.Diff(_allTheTypes, result), Is.EqualTo(ExpectedDiffs));
		}

		[Test]
		public void RewriteQualification()
		{
			var result = _allTheTypes.Apply(DummyRewriter.Instance, x => Replace(x, "Culture", "AnotherCulture"));

			const string ExpectedDiffs =
@"ByRef:
  Pointer:
    ArrayType:
      NamedType:
        ""TypeName""
        Assembly:
          ""Foo""
          Qualification:
<<<<<<<
            ""Culture""
=======
            ""AnotherCulture""
>>>>>>>
            ""neutral""
        NamedType:
          ""TArg""
      1
";

			Assert.That(TreeDiff.Diff(_allTheTypes, result), Is.EqualTo(ExpectedDiffs));
		}

		[Test]
		public void RewriteTypeArg()
		{
			var result = _allTheTypes.Apply(DummyRewriter.Instance, x => Replace(x, "TArg", "TArgument"));

			const string ExpectedDiffs =
@"ByRef:
  Pointer:
    ArrayType:
      NamedType:
        ""TypeName""
        Assembly:
          ""Foo""
          Qualification:
            ""Culture""
            ""neutral""
        NamedType:
<<<<<<<
          ""TArg""
=======
          ""TArgument""
>>>>>>>
      1
";

			Assert.That(TreeDiff.Diff(_allTheTypes, result), Is.EqualTo(ExpectedDiffs));
		}

		string Replace(string text, string oldValue, string newValue) => text == oldValue ? newValue : text;

		readonly InsType _allTheTypes =
			ByRefType(
				PointerType(
					ArrayType(
						NamedType(
							"TypeName",
							Assembly(
								"Foo",
								Qualification("Culture", "neutral")),
							NamedType("TArg")),
						1)));

		sealed class DummyRewriter : InsRewriter<Func<string, string>>
		{
			public static DummyRewriter Instance { get; } = new DummyRewriter();

			public override InsType VisitNamed(InsNamedType type, Func<string, string> mutator)
			{
				var newName = mutator(type.Name);

				if (newName != type.Name)
				{
					return NamedType(
						newName,
						type.Assembly == null ? null : VisitAssembly(type.Assembly, mutator),
						VisitTypes(type.TypeArguments, mutator));
				}

				return base.VisitNamed(type, mutator);
			}

			public override InsAssembly VisitAssembly(InsAssembly assembly, Func<string, string> mutator)
			{
				var result = base.VisitAssembly(assembly, mutator);

				var newName = mutator(assembly.Name);

				if (newName != assembly.Name)
				{
					return Assembly(newName, assembly.Qualifications);
				}

				return result;
			}

			public override InsAssemblyQualification VisitAssemblyQualification(InsAssemblyQualification qualification, Func<string, string> mutator)
			{
				var newName = mutator(qualification.Name);
				var newValue = mutator(qualification.Value);

				if (newName == qualification.Name && newValue == qualification.Value)
				{
					return qualification;
				}

				return Qualification(newName, newValue);
			}
		}
	}
}
