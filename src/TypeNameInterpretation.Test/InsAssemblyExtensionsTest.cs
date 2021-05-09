using System;
using NUnit.Framework;

namespace TypeNameInterpretation.Test
{
	[TestFixture]
	class InsAssemblyExtensionsTest
	{
		[TestCase("Foo, Version=4.2.0.0", "4.2.0.0")]
		[TestCase("Foo", null)]
		public void GetVersion(string assemblyName, string expectedVersion)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);

			if (expectedVersion == null)
			{
				Assert.That(assembly.TryGetVersion(out var version), Is.False);
				Assert.That(version, Is.Null);
			}
			else
			{
				Assert.That(assembly.TryGetVersion(out var version), Is.True);
				Assert.That(version, Is.EqualTo(Version.Parse(expectedVersion)));
			}
		}

		[TestCase("Foo, Version=invalid")]
		public void GetVersion_Invalid(string assemblyName)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(
				() => assembly.TryGetVersion(out _),
				Throws.Exception.TypeOf<FormatException>()
					.With.Message.EqualTo("Version qualification was provided, but was in an unrecognised format."));
		}

		[TestCase("Foo, PublicKeyToken=0123456789ABCDEF", true, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF })]
		[TestCase("Foo, PublicKeyToken=null", true, null)]
		[TestCase("Foo", false, null)]
		public void GetPublicKeyToken(string assemblyName, bool success, byte[] token)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);

			if (success)
			{
				Assert.That(assembly.TryGetPublicKeyToken(out var value), Is.True);
				Assert.That(value, Is.EqualTo(token));
			}
			else
			{
				Assert.That(assembly.TryGetPublicKeyToken(out var value), Is.False);
				Assert.That(value, Is.Null);
			}
		}

		[TestCase("Foo, PublicKeyToken=0123456789ABCDE")]
		public void GetPublicKeyToken_Invalid(string assemblyName)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(
				() => assembly.TryGetPublicKeyToken(out _),
				Throws.Exception.TypeOf<FormatException>()
					.With.Message.EqualTo("PublicKeyToken qualification was provided, but was in an unrecognised format."));
		}

		[TestCase("Foo, PublicKey=0123456789ABCDEF", true, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF })]
		[TestCase("Foo, PublicKey=null", true, null)]
		[TestCase("Foo", false, null)]
		public void GetPublicKey(string assemblyName, bool success, byte[] token)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);

			if (success)
			{
				Assert.That(assembly.TryGetPublicKey(out var value), Is.True);
				Assert.That(value, Is.EqualTo(token));
			}
			else
			{
				Assert.That(assembly.TryGetPublicKey(out var value), Is.False);
				Assert.That(value, Is.Null);
			}
		}

		[TestCase("Foo, PublicKey=0123456789ABCDE")]
		public void GetPublicKey_Invalid(string assemblyName)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			Assert.That(
				() => assembly.TryGetPublicKey(out _),
				Throws.Exception.TypeOf<FormatException>()
					.With.Message.EqualTo("PublicKey qualification was provided, but was in an unrecognised format."));
		}

		[TestCase("Bar", ExpectedResult = "A")]
		[TestCase("Baz", ExpectedResult = "B")]
		[TestCase("Missing", ExpectedResult = null)]
		public string? TryGetQualification(string name)
		{
			var assembly = InsTypeFactory.ParseAssemblyName("Foo, Bar=A, Baz=B, Quux=C");

			if (!assembly.TryGetQualification(name, out var result))
			{
				return null;
			}

			return result ?? "<null>";
		}

		[TestCase("Foo", "1.0", ExpectedResult = "Foo, Version=1.0")]
		[TestCase("Foo, Version=1.0", "1.0", ExpectedResult = "Foo, Version=1.0")]
		[TestCase("Foo, Version=1.0", "2.0", ExpectedResult = "Foo, Version=2.0")]
		public string WithVersion(string assemblyName, string version)
		{
			return InsFormatter.Format(
				InsTypeFactory.ParseAssemblyName(assemblyName)
					.WithVersion(Version.Parse(version)));
		}

		[TestCase("Foo", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, ExpectedResult = "Foo, PublicKeyToken=0123456789ABCDEF")]
		[TestCase("Foo, PublicKeyToken=0123456789ABCDEF", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, ExpectedResult = "Foo, PublicKeyToken=0123456789ABCDEF")]
		[TestCase("Foo, PublicKeyToken=0123456789ABCDEF", new byte[] { 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 }, ExpectedResult = "Foo, PublicKeyToken=89ABCDEF01234567")]
		[TestCase("Foo, PublicKeyToken=0123456789ABCDEF", null, ExpectedResult = "Foo, PublicKeyToken=null")]
		public string WithPublicKeyToken(string assemblyName, byte[] publicKey)
		{
			return InsFormatter.Format(
				InsTypeFactory.ParseAssemblyName(assemblyName)
					.WithPublicKeyToken(publicKey));
		}

		[TestCase("Foo", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, ExpectedResult = "Foo, PublicKey=0123456789ABCDEF")]
		[TestCase("Foo, PublicKey=0123456789ABCDEF", new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF }, ExpectedResult = "Foo, PublicKey=0123456789ABCDEF")]
		[TestCase("Foo, PublicKey=0123456789ABCDEF", new byte[] { 0x89, 0xAB, 0xCD, 0xEF, 0x01, 0x23, 0x45, 0x67 }, ExpectedResult = "Foo, PublicKey=89ABCDEF01234567")]
		[TestCase("Foo, PublicKey=0123456789ABCDEF", null, ExpectedResult = "Foo, PublicKey=null")]
		public string WithPublicKey(string assemblyName, byte[] publicKey)
		{
			return InsFormatter.Format(
				InsTypeFactory.ParseAssemblyName(assemblyName)
					.WithPublicKey(publicKey));
		}

		[TestCase("Foo", "Bar", "value", ExpectedResult = "Foo, Bar=value")]
		[TestCase("Foo, Baz=A, Quux=B", "Bar", "value", ExpectedResult = "Foo, Baz=A, Quux=B, Bar=value")]
		[TestCase("Foo, Baz=A, Bar=C, Quux=B", "Bar", "value", ExpectedResult = "Foo, Baz=A, Bar=value, Quux=B")]
		public string WithQualification(string assemblyName, string name, string value)
		{
			return InsFormatter.Format(
				InsTypeFactory.ParseAssemblyName(assemblyName)
					.WithQualification(name, value));
		}

		[TestCase("Foo, Bar=value", "Bar", "value")]
		public void WithQualification_Unchanged(string assemblyName, string name, string value)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			var newAssembly = assembly.WithQualification(name, value);
			Assert.That(newAssembly, Is.SameAs(assembly));
		}

		[TestCase("Foo", "Bar", ExpectedResult = "Foo")]
		[TestCase("Foo, Bar=A", "Bar", ExpectedResult = "Foo")]
		[TestCase("Foo, Baz=A, Bar=B, Quux=C", "Bar", ExpectedResult = "Foo, Baz=A, Quux=C")]
		public string WithoutQualification(string assemblyName, string name)
		{
			return InsFormatter.Format(
				InsTypeFactory.ParseAssemblyName(assemblyName)
					.WithoutQualification(name));
		}

		[TestCase("Foo", "Bar")]
		[TestCase("Foo, Baz=A", "Bar")]
		public void WithoutQualification_Unchanged(string assemblyName, string name)
		{
			var assembly = InsTypeFactory.ParseAssemblyName(assemblyName);
			var newAssembly = assembly.WithoutQualification(name);
			Assert.That(newAssembly, Is.SameAs(assembly));
		}
	}
}
