using System;
using NUnit.Framework;

namespace TypeNameInterpretation.Test
{
	[TestFixture]
	class InsAssemblyExtensionsTest
	{
		[TestCase("Foo, Version=4.2.0.0", "4.2.0.0")]
		[TestCase("Foo, Version=invalid", null)]
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

		[TestCase("Foo, PublicKeyToken=0123456789ABCDEF", true, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF })]
		[TestCase("Foo, PublicKeyToken=0123456789ABCDE", false, null)]
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

		[TestCase("Foo, PublicKey=0123456789ABCDEF", true, new byte[] { 0x01, 0x23, 0x45, 0x67, 0x89, 0xAB, 0xCD, 0xEF })]
		[TestCase("Foo, PublicKey=0123456789ABCDE", false, null)]
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
