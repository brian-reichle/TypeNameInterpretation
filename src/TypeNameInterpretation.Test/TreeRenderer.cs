// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Collections.Immutable;
using System.Text;

namespace TypeNameInterpretation.Test
{
	static class TreeRenderer
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
			new Context(builder).FormatType('\0', 0, type);
			return builder.ToString();
		}

		const char Value1 = '-';
		const char Value2 = '+';
		const char Similar = '~';
		const char Same = ' ';

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
					FormatType(Same, indent, type1);
					return;
				}

				if (type1 == null || type2 == null || type1.Kind != type2.Kind)
				{
					FormatType(Value1, indent, type1);
					FormatType(Value2, indent, type2);
					return;
				}

				switch (type1.Kind)
				{
					case InsTypeKind.Array:
						DiffArrayType(indent, (InsArrayType)type1, (InsArrayType)type2);
						break;

					case InsTypeKind.ByRef:
						DiffByRefType(indent, (InsByRefType)type1, (InsByRefType)type2);
						break;

					case InsTypeKind.Generic:
						DiffGenericType(indent, (InsGenericType)type1, (InsGenericType)type2);
						break;

					case InsTypeKind.Named:
						DiffNamedType(indent, (InsNamedType)type1, (InsNamedType)type2);
						break;

					case InsTypeKind.Pointer:
						DiffPointerType(indent, (InsPointerType)type1, (InsPointerType)type2);
						break;

					case InsTypeKind.SZArray:
						DiffSZArray(indent, (InsSZArrayType)type1, (InsSZArrayType)type2);
						break;
				}
			}

			void DiffArrayType(int indent, InsArrayType type1, InsArrayType type2)
			{
				FormatLabel(Similar, indent, "ArrayType");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
				DiffLiteral(indent, type1.Rank, type2.Rank);
			}

			void DiffByRefType(int indent, InsByRefType type1, InsByRefType type2)
			{
				FormatLabel(Similar, indent, "ByRef");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffGenericType(int indent, InsGenericType type1, InsGenericType type2)
			{
				FormatLabel(Similar, indent, "Generic");

				indent++;
				DiffType(indent, type1.Definition, type2.Definition);
				DiffTypes(indent, type1.TypeArguments, type2.TypeArguments);
			}

			void DiffNamedType(int indent, InsNamedType type1, InsNamedType type2)
			{
				FormatLabel(Similar, indent, "NamedType");

				indent++;
				DiffLiteral(indent, type1.Name, type2.Name);
				DiffType(indent, type1.DeclaringType, type2.DeclaringType);
				DiffAssembly(indent, type1.Assembly, type2.Assembly);
			}

			void DiffPointerType(int indent, InsPointerType type1, InsPointerType type2)
			{
				FormatLabel(Similar, indent, "Pointer");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffSZArray(int indent, InsSZArrayType type1, InsSZArrayType type2)
			{
				FormatLabel(Similar, indent, "SZArrayType");

				indent++;
				DiffType(indent, type1.ElementType, type2.ElementType);
			}

			void DiffTypes(int indent, ImmutableArray<InsType> types1, ImmutableArray<InsType> types2)
			{
				if (types1 == types2)
				{
					FormatTypes(Same, indent, types1);
				}
				else
				{
					for (var i = 0; i < types1.Length; i++)
					{
						DiffType(indent, types1[i], types2[i]);
					}
				}
			}

			void DiffAssembly(int indent, InsAssembly? assembly1, InsAssembly? assembly2)
			{
				if (ReferenceEquals(assembly1, assembly2))
				{
					FormatAssembly(Same, indent, assembly1);
				}
				else if (assembly1 == null || assembly2 == null)
				{
					FormatAssembly(Value1, indent, assembly1);
					FormatAssembly(Value2, indent, assembly2);
				}
				else
				{
					FormatLabel(Similar, indent, "Assembly");

					indent++;
					DiffLiteral(indent, assembly1.Name, assembly2.Name);
					DiffQualifications(indent, assembly1.Qualifications, assembly2.Qualifications);
				}
			}

			void DiffQualifications(int indent, ImmutableArray<InsAssemblyQualification> qualifications1, ImmutableArray<InsAssemblyQualification> qualifications2)
			{
				if (qualifications1 == qualifications2)
				{
					FormatQualifications(Same, indent, qualifications1);
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
				if (ReferenceEquals(qualification1, qualification2))
				{
					FormatQualification(Same, indent, qualification1);
				}
				else
				{
					FormatLabel(Similar, indent, "Qualification");

					indent++;
					DiffLiteral(indent, qualification1.Name, qualification2.Name);
					DiffLiteral(indent, qualification1.Value, qualification2.Value);
				}
			}

			void DiffLiteral(int indent, string value1, string value2)
			{
				if (ReferenceEquals(value1, value2))
				{
					FormatLiteral(Same, indent, value1);
				}
				else if (value1 == value2)
				{
					FormatLiteral(Similar, indent, value1);
				}
				else
				{
					FormatLiteral(Value1, indent, value1);
					FormatLiteral(Value2, indent, value2);
				}
			}

			void DiffLiteral(int indent, int value1, int value2)
			{
				if (value1 == value2)
				{
					FormatLiteral(Same, indent, value1);
				}
				else
				{
					FormatLiteral(Value1, indent, value1);
					FormatLiteral(Value2, indent, value2);
				}
			}

			public void FormatType(char prefix, int indent, InsType? type)
			{
				if (type == null)
				{
					return;
				}

				switch (type.Kind)
				{
					case InsTypeKind.Array:
						FormatArray(prefix, indent, (InsArrayType)type);
						break;

					case InsTypeKind.ByRef:
						FormatByRef(prefix, indent, (InsByRefType)type);
						break;

					case InsTypeKind.Generic:
						FormatGeneric(prefix, indent, (InsGenericType)type);
						break;

					case InsTypeKind.Named:
						FormatNamed(prefix, indent, (InsNamedType)type);
						break;

					case InsTypeKind.Pointer:
						FormatPointer(prefix, indent, (InsPointerType)type);
						break;

					case InsTypeKind.SZArray:
						FormatSZArray(prefix, indent, (InsSZArrayType)type);
						break;

					default:
						throw new ArgumentException("Unknown type kind: " + type.Kind, nameof(type));
				}
			}

			void FormatArray(char prefix, int indent, InsArrayType type)
			{
				FormatLabel(prefix, indent, "ArrayType");

				indent++;
				FormatType(prefix, indent, type.ElementType);
				FormatLiteral(prefix, indent, type.Rank);
			}

			void FormatByRef(char prefix, int indent, InsByRefType type)
			{
				FormatLabel(prefix, indent, "ByRef");

				indent++;
				FormatType(prefix, indent, type.ElementType);
			}

			void FormatGeneric(char prefix, int indent, InsGenericType type)
			{
				FormatLabel(prefix, indent, "Generic");

				indent++;
				FormatType(prefix, indent, type.Definition);
				FormatTypes(prefix, indent, type.TypeArguments);
			}

			void FormatNamed(char prefix, int indent, InsNamedType type)
			{
				FormatLabel(prefix, indent, "NamedType");

				indent++;
				FormatLiteral(prefix, indent, type.Name);
				FormatType(prefix, indent, type.DeclaringType);
				FormatAssembly(prefix, indent, type.Assembly);
			}

			void FormatPointer(char prefix, int indent, InsPointerType type)
			{
				FormatLabel(prefix, indent, "Pointer");

				indent++;
				FormatType(prefix, indent, type.ElementType);
			}

			void FormatSZArray(char prefix, int indent, InsSZArrayType type)
			{
				FormatLabel(prefix, indent, "SZArrayType");

				indent++;
				FormatType(prefix, indent, type.ElementType);
			}

			void FormatTypes(char prefix, int indent, ImmutableArray<InsType> types)
			{
				foreach (var typeArg in types)
				{
					FormatType(prefix, indent, typeArg);
				}
			}

			void FormatAssembly(char prefix, int indent, InsAssembly? assembly)
			{
				if (assembly != null)
				{
					FormatLabel(prefix, indent, "Assembly");

					indent++;
					FormatLiteral(prefix, indent, assembly.Name);
					FormatQualifications(prefix, indent, assembly.Qualifications);
				}
			}

			void FormatQualifications(char prefix, int indent, ImmutableArray<InsAssemblyQualification> qualifications)
			{
				foreach (var qualification in qualifications)
				{
					FormatQualification(prefix, indent, qualification);
				}
			}

			void FormatQualification(char prefix, int indent, InsAssemblyQualification qualification)
			{
				FormatLabel(prefix, indent, "Qualification");

				indent++;
				FormatLiteral(prefix, indent, qualification.Name);
				FormatLiteral(prefix, indent, qualification.Value);
			}

			void FormatLabel(char prefix, int indent, string label)
			{
				WriteIndent(prefix, indent);
				_builder.Append(label).Append(':').AppendLine();
			}

			void FormatLiteral(char prefix, int indent, string value)
			{
				WriteIndent(prefix, indent);

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

			void FormatLiteral(char prefix, int indent, int value)
			{
				WriteIndent(prefix, indent);
				_builder
					.Append(value)
					.AppendLine();
			}

			void WriteIndent(char prefix, int indent)
			{
				if (prefix != '\0')
				{
					_builder.Append(prefix);
				}

				_builder.Append(' ', indent * 2);
			}

			readonly StringBuilder _builder;
		}
	}
}
