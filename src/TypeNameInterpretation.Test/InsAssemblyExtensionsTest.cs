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
	}
}
