// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Runtime.Serialization;

namespace TypeNameInterpretation
{
	[Serializable]
	public sealed class InvalidTypeNameException : FormatException
	{
		public InvalidTypeNameException()
		{
		}

		public InvalidTypeNameException(string message)
			: base(message)
		{
		}

		public InvalidTypeNameException(string message, Exception innerException)
			: base(message, innerException)
		{
		}

		InvalidTypeNameException(SerializationInfo info, StreamingContext context)
			: base(info, context)
		{
		}
	}
}
