// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using BenchmarkDotNet.Attributes;

namespace TypeNameInterpretation.Benchmark;

[MemoryDiagnoser]
public class AssemblyNameParsing
{
	[Params("Foo", "Foo, Bar=\"Qux Quux\"", "Foo, Bar=\"Qux \\\"Quux\"")]
	public string Text { get; set; }

	[Benchmark]
	public InsAssembly Parse() => InsTypeFactory.ParseAssemblyName(Text);
}
