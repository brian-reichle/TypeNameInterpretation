namespace TypeNameInterpretation
{
	public interface IInsTypeVisitor<TArgument, TReturn>
	{
		TReturn VisitArray(InsArrayType type, TArgument argument);
		TReturn VisitByRef(InsByRefType type, TArgument argument);
		TReturn VisitGeneric(InsGenericType type, TArgument argument);
		TReturn VisitNamed(InsNamedType type, TArgument argument);
		TReturn VisitPointer(InsPointerType type, TArgument argument);
		TReturn VisitSZArray(InsSZArrayType type, TArgument argument);
	}
}
