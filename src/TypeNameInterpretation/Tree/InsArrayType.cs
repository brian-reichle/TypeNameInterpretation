using System;

namespace TypeNameInterpretation
{
	public sealed class InsArrayType : InsType
	{
		internal InsArrayType(InsType elementType, int rank)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
			Rank = rank;
		}

		public InsType ElementType { get; }
		public int Rank { get; }
		public override InsTypeKind Kind => InsTypeKind.Array;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitArray(this, argument);
		}
	}
}
