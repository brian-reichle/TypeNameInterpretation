// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Generic;
using NUnit.Framework;

namespace TypeNameInterpretation.Test
{
	[TestFixture]
	class RuntimeCompatibilityTest
	{
		[TestCase(typeof(int))]
		[TestCase(typeof(InsTypeFactory))]
		[TestCase(typeof(List<>))]
		[TestCase(typeof(List<int>))]
		[TestCase(typeof(List<int>.Enumerator))]
		[TestCase(typeof(Dictionary<string, List<int>>))]
		[TestCase(typeof(string[]))]
		[TestCase(typeof(string[,][]))]
		[TestCaseSource(nameof(AdditionalTypes))]
		public void RoundTrip(Type type)
		{
			// We want to assert that InsTypeFactory.ParseTypeName should be able to handle whatever Type.AssemblyQualifiedName produces and
			// InsFormatter.Format should produce something that Type.GetType can understand, but we don't want to tie ourselves to any
			// particular assembly name or qualifications that a given target framework uses. We could also assert that we format the type
			// name in the same way as AssemblyQualifiedName, but then we run the risk of breaking if different frameworks format it
			// differently.
			var insType = InsTypeFactory.ParseTypeName(type.AssemblyQualifiedName!);
			var resultType = Type.GetType(InsFormatter.Format(insType));
			Assert.That(resultType, Is.EqualTo(type));
		}

		protected static IEnumerable<TestCaseData> AdditionalTypes
		{
			get
			{
				var baseType = typeof(int);

				yield return Test(baseType.MakeArrayType(1));
				yield return Test(baseType.MakePointerType());
				yield return Test(baseType.MakeByRefType());

				static TestCaseData Test(Type type) => new(type);
			}
		}
	}
}
