using System;
using System.Collections.Immutable;

namespace TypeInterpretation
{
	public sealed class InsNamedType : InsType
	{
		internal InsNamedType(
			string name,
			InsAssembly? assembly,
			ImmutableArray<InsType> typeArguments)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Assembly = assembly;
			TypeArguments = typeArguments;
		}

		public string Name { get; }
		public InsAssembly? Assembly { get; }
		public ImmutableArray<InsType> TypeArguments { get; }
		public override InsTypeKind Kind => InsTypeKind.Named;

		public override TReturn Apply<TArgument, TReturn>(IInsTypeVisitor<TArgument, TReturn> visitor, TArgument argument)
		{
			if (visitor == null)
			{
				throw new ArgumentNullException(nameof(visitor));
			}

			return visitor.VisitNamed(this, argument);
		}
	}
}
