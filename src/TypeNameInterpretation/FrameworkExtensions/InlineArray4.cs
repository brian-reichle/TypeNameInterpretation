// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
#if NET8_0 || NET9_0
namespace System.Runtime.CompilerServices;

// Inline arrays require runtime support which wasn't added until net8.0, so we can't use this in earlier frameworks.
// net10.0 added it's own implementation of `InlineArray4<>` so we don't need this when targeting later frameworks.
[InlineArray(4)]
struct InlineArray4<T>
{
	T _value;
}
#endif
