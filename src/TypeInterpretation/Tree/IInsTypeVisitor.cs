namespace TypeInterpretation
{
	public interface IInsTypeVisitor<TArgument, TReturn>
	{
		TReturn VisitArray(InsArrayType type, TArgument argument);
		TReturn VisitNamed(InsNamedType type, TArgument argument);
		TReturn VisitPointer(InsPointerType type, TArgument argument);
		TReturn VisitByRef(InsByRefType type, TArgument argument);
	}
}
