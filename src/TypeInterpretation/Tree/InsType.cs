namespace TypeInterpretation
{
	public abstract class InsType
	{
		private protected InsType()
		{
		}

		public abstract InsTypeKind Kind { get; }
		public override string ToString() => InsFormatter.Format(this);
		public abstract TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument);
	}
}
