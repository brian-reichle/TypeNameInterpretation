using System;

namespace TypeNameInterpretation
{
	public abstract class InsElementedType : InsType
	{
		private protected InsElementedType(InsType elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public InsType ElementType { get; }
	}
}
