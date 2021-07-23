using System;

namespace TypeNameInterpretation
{
	public sealed class InsByRefType : InsElementedType
	{
		internal InsByRefType(InsType elementType)
			: base(elementType)
		{
		}

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
