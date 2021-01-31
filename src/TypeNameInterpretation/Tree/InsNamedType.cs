using System;
using System.Collections.Immutable;

namespace TypeInterpretation
{
	public sealed class InsNamedType : InsType
	{
		internal InsNamedType(
			string name,
			InsAssembly? assembly)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			Assembly = assembly;
		}

		internal InsNamedType(
			string name,
			InsNamedType? declaringType)
		{
			Name = name ?? throw new ArgumentNullException(nameof(name));
			DeclaringType = declaringType ?? throw new ArgumentNullException(nameof(declaringType));
		}

		public string Name { get; }
		public InsNamedType? DeclaringType { get; }
		public InsAssembly? Assembly { get; }
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
