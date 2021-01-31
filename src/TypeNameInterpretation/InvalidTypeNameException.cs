using System;
using System.Runtime.Serialization;

namespace TypeInterpretation
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
