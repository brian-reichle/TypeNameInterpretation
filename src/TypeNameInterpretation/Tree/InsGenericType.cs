using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation
{
	public sealed class InsGenericType : InsType
	{
		internal InsGenericType(InsNamedType definition, ImmutableArray<InsType> typeArguments)
		{
			Definition = definition ?? throw new ArgumentNullException(nameof(definition));
			TypeArguments = typeArguments;
		}

		public InsNamedType Definition { get; }
		public ImmutableArray<InsType> TypeArguments { get; }
		public override InsTypeKind Kind => InsTypeKind.Generic;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitGeneric(this, argument);
		}
	}
}
