using System;
using System.Text;

namespace TypeInterpretation
{
	public static class InsFormatter
	{
		public static StringBuilder Write(StringBuilder builder, InsType type)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			if (type == null)
			{
				throw new ArgumentNullException(nameof(type));
			}

			return Writer.Instance.WriteComplexType(type, builder);
		}

		public static StringBuilder Write(StringBuilder builder, InsAssembly assembly)
		{
			if (builder == null)
			{
				throw new ArgumentNullException(nameof(builder));
			}

			if (assembly == null)
			{
				throw new ArgumentNullException(nameof(assembly));
			}

			return Writer.WriteAssembly(assembly, builder);
		}

		public static string Format(InsType type) => Write(new StringBuilder(), type).ToString();
		public static string Format(InsAssembly assembly) => Write(new StringBuilder(), assembly).ToString();

		sealed class Writer : IInsTypeVisitor<StringBuilder, StringBuilder>
		{
			public static Writer Instance { get; } = new Writer();

			Writer()
			{
			}

			public StringBuilder WriteComplexType(InsType type, StringBuilder builder)
			{
				type.Apply(this, builder);

				var assembly = type.Apply(AssemblyLocator.Instance, builder);

				if (assembly != null)
				{
					builder.Append(", ");
					WriteAssembly(assembly, builder);
				}

				return builder;
			}

			public StringBuilder VisitArray(InsArrayType type, StringBuilder builder)
				=> type.ElementType.Apply(this, builder).Append('[').Append(',', type.Rank - 1).Append(']');

			public StringBuilder VisitNamed(InsNamedType type, StringBuilder builder)
			{
				if (type.DeclaringType != null)
				{
					type.DeclaringType.Apply(this, builder);
					builder.Append('+');
				}

				WriteIdentifier(type.Name, builder);

				return builder;
			}

			public StringBuilder VisitPointer(InsPointerType type, StringBuilder builder)
				=> type.ElementType.Apply(this, builder).Append('*');

			public StringBuilder VisitByRef(InsByRefType type, StringBuilder builder)
				=> type.ElementType.Apply(this, builder).Append('&');

			public StringBuilder VisitGeneric(InsGenericType type, StringBuilder builder)
			{
				type.Definition.Apply(this, builder);

				var typeArguments = type.TypeArguments;

				if (typeArguments.Length > 0)
				{
					builder.Append("[[");
					WriteComplexType(typeArguments[0], builder);

					for (var i = 1; i < typeArguments.Length; i++)
					{
						builder.Append("],[");
						WriteComplexType(typeArguments[i], builder);
					}

					builder.Append("]]");
				}

				return builder;
			}

			public static StringBuilder WriteAssembly(InsAssembly assembly, StringBuilder builder)
			{
				WriteIdentifier(assembly.Name, builder);

				foreach (var qualification in assembly.Qualifications)
				{
					builder
						.Append(", ")
						.Append(qualification.Name)
						.Append('=')
						.Append(qualification.Value);
				}

				return builder;
			}

			static void WriteIdentifier(string identifier, StringBuilder builder)
			{
				var start = 0;

				for (var i = 0; i < identifier.Length; i++)
				{
					switch (identifier[i])
					{
						case ',':
						case '[':
						case ']':
						case '&':
						case '*':
						case '+':
						case '\\':
							break;

						default:
							continue;
					}

					builder
						.Append(identifier, start, i - start)
						.Append('\\');

					start = i;
				}

				builder.Append(identifier, start, identifier.Length - start);
			}
		}

		sealed class AssemblyLocator : IInsTypeVisitor<object, InsAssembly?>
		{
			public static AssemblyLocator Instance { get; } = new AssemblyLocator();

			AssemblyLocator()
			{
			}

			public InsAssembly? VisitArray(InsArrayType type, object argument) => type.ElementType.Apply(this, argument);
			public InsAssembly? VisitPointer(InsPointerType type, object argument) => type.ElementType.Apply(this, argument);
			public InsAssembly? VisitByRef(InsByRefType type, object argument) => type.ElementType.Apply(this, argument);
			public InsAssembly? VisitGeneric(InsGenericType type, object argument) => VisitNamed(type.Definition, argument);

			public InsAssembly? VisitNamed(InsNamedType type, object argument)
			{
				while (type.DeclaringType != null)
				{
					type = type.DeclaringType;
				}

				return type.Assembly;
			}
		}
	}
}
