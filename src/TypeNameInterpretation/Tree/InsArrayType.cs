using System;

namespace TypeNameInterpretation
{
	public sealed class InsArrayType : InsElementedType
	{
		internal InsArrayType(InsType elementType, int rank)
			: base(elementType)
		{
			Rank = rank;
		}

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
