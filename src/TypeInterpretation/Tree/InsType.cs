namespace TypeInterpretation
{
	public abstract class InsType
	{
		private protected InsType()
		{
		}

		public abstract InsTypeKind Kind { get; }
		public abstract TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument);
	}
}
