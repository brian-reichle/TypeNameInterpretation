// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using BenchmarkDotNet.Attributes;
using static TypeNameInterpretation.InsTypeFactory;

namespace TypeNameInterpretation.Benchmark;

[MemoryDiagnoser]
public class FormatAssemblyName
{
	[ParamsSource(nameof(TypesToFormat))]
	public InsAssembly Type { get; set; }

	public static IEnumerable<InsAssembly> TypesToFormat()
	{
		return new[]
		{
			Assembly("Foo"),
			Assembly("Foo", Qualification("Bar", "Qux Quux")),
			Assembly("Foo", Qualification("Bar", "Qux \"Quux")),
		};
	}

	[Benchmark]
	public string Format() => InsFormatter.Format(Type);
}
