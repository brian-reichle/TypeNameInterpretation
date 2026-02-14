// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
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
			ArgumentNullException.ThrowIfNull(visitor);
			return visitor.VisitSZArray(this, argument);
		}
	}
}
