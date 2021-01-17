using System;
using System.Collections.Immutable;

namespace TypeInterpretation
{
	public abstract class InsRewriter<TArgument> : IInsTypeVisitor<TArgument, InsType>
	{
		public virtual InsType VisitArray(InsArrayType type, TArgument argument)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var elementType = type.ElementType.Apply(this, argument);

			if (elementType == type.ElementType)
			{
				return type;
			}

			return new InsArrayType(elementType, type.Rank);
		}

		public virtual InsType VisitNamed(InsNamedType type, TArgument argument)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			if (type.DeclaringType != null)
			{
				var declaringType = (InsNamedType)type.DeclaringType.Apply(this, argument);

				if (declaringType == type.DeclaringType)
				{
					return type;
				}

				return new InsNamedType(type.Name, declaringType);
			}
			else
			{
				var assembly = type.Assembly == null ? null : VisitAssembly(type.Assembly, argument);

				if (assembly == type.Assembly)
				{
					return type;
				}

				return new InsNamedType(type.Name, assembly);
			}
		}

		public virtual InsType VisitPointer(InsPointerType type, TArgument argument)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var inner = type.ElementType.Apply(this, argument);

			if (inner == type.ElementType)
			{
				return type;
			}

			return new InsPointerType(inner);
		}

		public virtual InsType VisitByRef(InsByRefType type, TArgument argument)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var inner = type.ElementType.Apply(this, argument);

			if (inner == type.ElementType)
			{
				return type;
			}

			return new InsByRefType(inner);
		}

		public virtual InsType VisitGeneric(InsGenericType type, TArgument argument)
		{
			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			var definition = (InsNamedType)type.Definition.Apply(this, argument);
			var typeArguments = VisitTypes(type.TypeArguments, argument);

			if (definition == type.Definition && typeArguments == type.TypeArguments)
			{
				return type;
			}

			return new InsGenericType(definition, typeArguments);
		}

		public virtual ImmutableArray<InsType> VisitTypes(ImmutableArray<InsType> typeArguments, TArgument argument)
		{
			ImmutableArray<InsType>.Builder? builder = null;

			for (var i = 0; i < typeArguments.Length; i++)
			{
				var type = typeArguments[i].Apply(this, argument);

				if (builder != null)
				{
					builder[i] = type;
				}
				else if (type != typeArguments[i])
				{
					builder = ImmutableArray.CreateBuilder<InsType>(typeArguments.Length);
					builder.AddRange(typeArguments, i);
					builder.Count = typeArguments.Length;
					builder[i] = type;
				}
			}

			return builder == null ? typeArguments : builder.MoveToImmutable();
		}

		public virtual InsAssembly VisitAssembly(InsAssembly assembly, TArgument argument)
		{
			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			var qualifications = VisitAssemblyQualifications(assembly.Qualifications, argument);

			if (qualifications == assembly.Qualifications)
			{
				return assembly;
			}

			return new InsAssembly(assembly.Name, qualifications);
		}

		public virtual ImmutableArray<InsAssemblyQualification> VisitAssemblyQualifications(ImmutableArray<InsAssemblyQualification> qualifications, TArgument argument)
		{
			ImmutableArray<InsAssemblyQualification>.Builder? builder = null;

			for (var i = 0; i < qualifications.Length; i++)
			{
				var type = VisitAssemblyQualification(qualifications[i], argument);

				if (builder != null)
				{
					builder[i] = type;
				}
				else if (type != qualifications[i])
				{
					builder = ImmutableArray.CreateBuilder<InsAssemblyQualification>(qualifications.Length);
					builder.AddRange(qualifications, i);
					builder.Count = qualifications.Length;
					builder[i] = type;
				}
			}

			return builder == null ? qualifications : builder.MoveToImmutable();
		}

		public virtual InsAssemblyQualification VisitAssemblyQualification(InsAssemblyQualification qualification, TArgument argument) => qualification;
	}
}
