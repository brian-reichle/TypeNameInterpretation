using System;
using System.Collections.Immutable;
using System.Text;

namespace TypeInterpretation.Test
{
	static class TreeDiff
	{
		public static string Diff(InsType type1, InsType type2)
		{
			var builder = new StringBuilder();
			new Context(builder).DiffType(0, type1, type2);
			return builder.ToString();
		}

		public static string Format(InsType type)
		{
			var builder = new StringBuilder();
			new Context(builder).FormatType(0, type);
			return builder.ToString();
		}

		struct Context
		{
			public Context(StringBuilder builder)
			{
				_builder = builder;
			}

			public void DiffType(int indent, InsType? type1, InsType? type2)
			{
				if (type1 == type2)
				{
					FormatType(indent, type1);
					return;
				}

				if (type1 == null || type2 == null || type1.Kind != type2.Kind)
				{
					StartLeft();
					FormatType(indent, type1);
					StartRight();
					FormatType(indent, type2);
					StartMatch();
					return;
				}

				switch (type1.Kind)
				{
					case InsTypeKind.Named:
						DiffNamedType(indent, (InsNamedType)type1, (InsNamedType)type2);
						break;

					case InsTypeKind.Array:
						DiffArrayType(indent, (InsArrayType)type1, (InsArrayType)type2);
						break;

					case InsTypeKind.ByRef:
						DiffByRefType(indent, (InsByRefType)type1, (InsByRefType)type2);
						break;

					case InsTypeKind.Pointer:
						DiffPointerType(indent, (InsPointerType)type1, (InsPointerType)type2);
						break;

					case InsTypeKind.Generic:
						DiffGenericType(indent, (InsGenericType)type1, (InsGenericType)type2);
						break;

					case InsTypeKind.SZArray:
						DiffSZArray(indent, (InsSZArrayType)type1, (InsSZArrayType)type2);
						break;
				}
			}

			void DiffSZArray(int indent, InsSZArrayType type1, InsSZArrayType type2)
			{
				FormatLabel(indent, "SZArrayType");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffTypes(int indent, ImmutableArray<InsType> types1, ImmutableArray<InsType> types2)
			{
				if (types1 == types2)
				{
					FormatTypes(indent, types1);
				}
				else
				{
					for (var i = 0; i < types1.Length; i++)
					{
						DiffType(indent, types1[i], types2[i]);
					}
				}
			}

			void DiffGenericType(int indent, InsGenericType type1, InsGenericType type2)
			{
				FormatLabel(indent, "Generic");

				indent++;
				DiffType(indent, type1.Definition, type2.Definition);
				DiffTypes(indent, type1.TypeArguments, type2.TypeArguments);
			}

			void DiffPointerType(int indent, InsPointerType type1, InsPointerType type2)
			{
				FormatLabel(indent, "Pointer");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffByRefType(int indent, InsByRefType type1, InsByRefType type2)
			{
				FormatLabel(indent, "ByRef");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffArrayType(int indent, InsArrayType type1, InsArrayType type2)
			{
				FormatLabel(indent, "ArrayType");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
				DiffLiteral(indent, type1.Rank, type2.Rank);
			}

			void DiffNamedType(int indent, InsNamedType type1, InsNamedType type2)
			{
				FormatLabel(indent, "NamedType");

				indent++;
				DiffLiteral(indent, type1.Name, type2.Name);
				DiffType(indent, type1.DeclaringType, type2.DeclaringType);
				DiffAssembly(indent, type1.Assembly, type2.Assembly);
			}

			void DiffAssembly(int indent, InsAssembly? assembly1, InsAssembly? assembly2)
			{
				if (assembly1 == assembly2)
				{
					FormatAssembly(indent, assembly1);
				}
				else if (assembly1 == null || assembly2 == null)
				{
					StartLeft();
					FormatAssembly(indent, assembly1);
					StartRight();
					FormatAssembly(indent, assembly2);
					StartMatch();
				}
				else
				{
					FormatLabel(indent, "Assembly");

					indent++;
					DiffLiteral(indent, assembly1.Name, assembly2.Name);
					DiffQualifications(indent, assembly1.Qualifications, assembly2.Qualifications);
				}
			}

			void DiffQualifications(int indent, ImmutableArray<InsAssemblyQualification> qualifications1, ImmutableArray<InsAssemblyQualification> qualifications2)
			{
				if (qualifications1 == qualifications2)
				{
					FormatQualifications(indent, qualifications1);
				}
				else
				{
					for (var i = 0; i < qualifications1.Length; i++)
					{
						DiffQualification(indent, qualifications1[i], qualifications2[i]);
					}
				}
			}

			void DiffQualification(int indent, InsAssemblyQualification qualification1, InsAssemblyQualification qualification2)
			{
				if (qualification1 == qualification2)
				{
					FormatQualification(indent, qualification1);
				}
				else
				{
					FormatLabel(indent, "Qualification");

					indent++;
					DiffLiteral(indent, qualification1.Name, qualification2.Name);
					DiffLiteral(indent, qualification1.Value, qualification2.Value);
				}
			}

			void DiffLiteral(int indent, string value1, string value2)
			{
				if (value1 == value2)
				{
					FormatLiteral(indent, value1);
				}
				else
				{
					StartLeft();
					FormatLiteral(indent, value1);
					StartRight();
					FormatLiteral(indent, value2);
					StartMatch();
				}
			}

			void DiffLiteral(int indent, int value1, int value2)
			{
				if (value1 == value2)
				{
					FormatLiteral(indent, value1);
				}
				else
				{
					StartLeft();
					FormatLiteral(indent, value1);
					StartRight();
					FormatLiteral(indent, value2);
					StartMatch();
				}
			}

			void FormatTypes(int indent, ImmutableArray<InsType> types)
			{
				foreach (var typeArg in types)
				{
					FormatType(indent, typeArg);
				}
			}

			public void FormatType(int indent, InsType? type)
			{
				if (type == null)
				{
					return;
				}

				switch (type.Kind)
				{
					case InsTypeKind.Named:
						FormatNamed(indent, (InsNamedType)type);
						break;

					case InsTypeKind.Array:
						FormatArray(indent, (InsArrayType)type);
						break;

					case InsTypeKind.ByRef:
						FormatByRef(indent, (InsByRefType)type);
						break;

					case InsTypeKind.Pointer:
						FormatPointer(indent, (InsPointerType)type);
						break;

					case InsTypeKind.Generic:
						FormatGeneric(indent, (InsGenericType)type);
						break;

					case InsTypeKind.SZArray:
						FormatSZArray(indent, (InsSZArrayType)type);
						break;

					default:
						throw new ArgumentException("Unknown type kind: " + type.Kind, nameof(type));
				}
			}

			void FormatSZArray(int indent, InsSZArrayType type)
			{
				FormatLabel(indent, "SZArrayType");

				indent++;
				FormatType(indent, type.ElementType);
			}

			void FormatGeneric(int indent, InsGenericType type)
			{
				FormatLabel(indent, "Generic");

				indent++;
				FormatType(indent, type.Definition);
				FormatTypes(indent, type.TypeArguments);
			}

			void FormatPointer(int indent, InsPointerType type)
			{
				FormatLabel(indent, "Pointer");

				indent++;
				FormatType(indent, type.ElementType);
			}

			void FormatByRef(int indent, InsByRefType type)
			{
				FormatLabel(indent, "ByRef");

				indent++;
				FormatType(indent, type.ElementType);
			}

			void FormatArray(int indent, InsArrayType type)
			{
				FormatLabel(indent, "ArrayType");

				indent++;
				FormatType(indent, type.ElementType);
				FormatLiteral(indent, type.Rank);
			}

			void FormatNamed(int indent, InsNamedType type)
			{
				FormatLabel(indent, "NamedType");

				indent++;
				FormatLiteral(indent, type.Name);
				FormatType(indent, type.DeclaringType);
				FormatAssembly(indent, type.Assembly);
			}

			void FormatAssembly(int indent, InsAssembly? assembly)
			{
				if (assembly != null)
				{
					FormatLabel(indent, "Assembly");

					indent++;
					FormatLiteral(indent, assembly.Name);
					FormatQualifications(indent, assembly.Qualifications);
				}
			}

			void FormatQualifications(int indent, ImmutableArray<InsAssemblyQualification> qualifications)
			{
				foreach (var qualification in qualifications)
				{
					FormatQualification(indent, qualification);
				}
			}

			void FormatQualification(int indent, InsAssemblyQualification qualification)
			{
				FormatLabel(indent, "Qualification");

				indent++;
				FormatLiteral(indent, qualification.Name);
				FormatLiteral(indent, qualification.Value);
			}

			void FormatLabel(int indent, string label)
			{
				WriteIndent(indent);
				_builder.Append(label).Append(':').AppendLine();
			}

			void FormatLiteral(int indent, string value)
			{
				WriteIndent(indent);

				if (value == null)
				{
					_builder.Append("null");
					return;
				}

				_builder
					.Append('"')
					.Append(value)
					.Append('"')
					.AppendLine();
			}

			void FormatLiteral(int indent, int value)
			{
				WriteIndent(indent);
				_builder
					.Append(value)
					.AppendLine();
			}

			void WriteIndent(int indent) => _builder.Append(' ', indent * 2);
			void StartLeft() => _builder.AppendLine("<<<<<<<");
			void StartRight() => _builder.AppendLine("=======");
			void StartMatch() => _builder.AppendLine(">>>>>>>");

			readonly StringBuilder _builder;
		}
	}
}
