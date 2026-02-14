// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
#if NETFRAMEWORK || NETSTANDARD
using System;
using System.Runtime.Serialization;

namespace TypeNameInterpretation;

[Serializable]
partial class InvalidTypeNameException
{
	InvalidTypeNameException(SerializationInfo info, StreamingContext context)
		: base(info, context)
	{
	}
}
#endif
