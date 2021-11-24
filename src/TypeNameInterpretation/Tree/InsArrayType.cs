// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
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
