using System;

namespace TypeNameInterpretation
{
	public sealed class InsSZArrayType : InsElementedType
	{
		internal InsSZArrayType(InsType elementType)
			: base(elementType)
		{
		}

		public override InsTypeKind Kind => InsTypeKind.SZArray;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitSZArray(this, argument);
		}
	}
}
