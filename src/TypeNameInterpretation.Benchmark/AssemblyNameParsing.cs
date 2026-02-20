// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Jobs;

namespace TypeNameInterpretation.Benchmark;

[SimpleJob(RuntimeMoniker.Net48)]
[SimpleJob(RuntimeMoniker.Net80)]
[SimpleJob(RuntimeMoniker.Net10_0)]
[MemoryDiagnoser]
public class AssemblyNameParsing
{
	[Params("Foo", "Foo, Bar=\"Qux Quux\"", "Foo, Bar=\"Qux \\\"Quux\"")]
	public string Text { get; set; }

	[Benchmark]
	public InsAssembly Parse() => InsTypeFactory.ParseAssemblyName(Text);
}
