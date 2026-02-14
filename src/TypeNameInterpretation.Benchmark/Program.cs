// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using BenchmarkDotNet.Running;

namespace TypeNameInterpretation.Benchmark;

static class Program
{
	static void Main(string[] args)
	{
		BenchmarkSwitcher
			.FromAssembly(typeof(Program).Assembly)
			.Run(args: args);
	}
}
