using System;

namespace TypeNameInterpretation
{
	public sealed class InsSZArrayType : InsType
	{
		internal InsSZArrayType(InsType elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public InsType ElementType { get; }
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
