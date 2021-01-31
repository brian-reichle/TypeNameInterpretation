using System;

namespace TypeNameInterpretation
{
	public sealed class InsPointerType : InsType
	{
		internal InsPointerType(InsType elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public InsType ElementType { get; }
		public override InsTypeKind Kind => InsTypeKind.Pointer;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitPointer(this, argument);
		}
	}
}
