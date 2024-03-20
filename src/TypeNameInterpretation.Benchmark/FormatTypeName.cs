// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using BenchmarkDotNet.Attributes;
using static TypeNameInterpretation.InsTypeFactory;

namespace TypeNameInterpretation.Benchmark
{
	[MemoryDiagnoser]
	public class FormatTypeName
	{
		[ParamsSource(nameof(TypesToFormat))]
		public InsType Type { get; set; }

		public static IEnumerable<InsType> TypesToFormat()
		{
			return new InsType[]
			{
				NamedType("Foo"),
				NamedType("Foo", Assembly("Bar", Qualification("Baz", "Qux Quux"))),
				Generic(NamedType("Foo"), NamedType("Bar"), NamedType("Baz")),
			};
		}

		[Benchmark]
		public string Format() => InsFormatter.Format(Type);
	}
}
