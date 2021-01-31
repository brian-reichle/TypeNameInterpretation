using System;

namespace TypeInterpretation
{
	public sealed class InsByRefType : InsType
	{
		internal InsByRefType(InsType elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public InsType ElementType { get; }
		public override InsTypeKind Kind => InsTypeKind.ByRef;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitByRef(this, argument);
		}
	}
}
