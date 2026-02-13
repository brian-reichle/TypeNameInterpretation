// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;

namespace TypeNameInterpretation
{
	public sealed class InsGenericType : InsType
	{
		internal InsGenericType(InsNamedType definition, ImmutableArray<InsType> typeArguments)
		{
			ArgumentNullException.ThrowIfNull(definition);
			Definition = definition;
			TypeArguments = typeArguments;
		}

		public InsNamedType Definition { get; }
		public ImmutableArray<InsType> TypeArguments { get; }
		public override InsTypeKind Kind => InsTypeKind.Generic;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			ArgumentNullException.ThrowIfNull(visitor);
			return visitor.VisitGeneric(this, argument);
		}
	}
}
