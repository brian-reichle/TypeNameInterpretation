// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
namespace TypeNameInterpretation;

public abstract class InsType
{
	private protected InsType()
	{
	}

	public abstract InsTypeKind Kind { get; }
	public override string ToString() => InsFormatter.Format(this);
	public abstract TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument);
}
