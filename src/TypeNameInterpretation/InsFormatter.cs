// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;
using System.Text;

#if NET
using System.Buffers;
#endif

namespace TypeNameInterpretation;

/// <summary>
/// Provides methods to format <see cref="InsType"/> and <see cref="InsAssembly"/> syntax trees into standard .NET representations.
/// </summary>
public static class InsFormatter
{
	/// <summary>
	/// Appends the formatted string representation of an <see cref="InsType"/> to a <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">The <see cref="StringBuilder"/> to write to.</param>
	/// <param name="type">The <see cref="InsType"/> to format.</param>
	/// <returns>The provided <paramref name="builder"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="type"/> is <see langword="null"/>.</exception>
	public static StringBuilder Write(StringBuilder builder, InsType type)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(type);
		return Writer.Instance.WriteComplexType(type, builder);
	}

	/// <summary>
	/// Appends the formatted string representation of an <see cref="InsAssembly"/> to a <see cref="StringBuilder"/>.
	/// </summary>
	/// <param name="builder">The <see cref="StringBuilder"/> to write to.</param>
	/// <param name="assembly">The <see cref="InsAssembly"/> to format.</param>
	/// <returns>The provided <paramref name="builder"/> instance.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="builder"/> or <paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static StringBuilder Write(StringBuilder builder, InsAssembly assembly)
	{
		ArgumentNullException.ThrowIfNull(builder);
		ArgumentNullException.ThrowIfNull(assembly);
		return Writer.WriteAssembly(assembly, builder);
	}

	/// <summary>
	/// Formats an <see cref="InsType"/> into its canonical string representation.
	/// </summary>
	/// <param name="type">The <see cref="InsType"/> to format.</param>
	/// <returns>A non-null formatted type name string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
	public static string Format(InsType type)
	{
		ArgumentNullException.ThrowIfNull(type);

		if (type.TryFastFormat(out var result))
		{
			return result;
		}

		return Write(BuilderPool.Rent(), type).ToStringAndReturn();
	}

	/// <summary>
	/// Formats an <see cref="InsAssembly"/> into its canonical string representation.
	/// </summary>
	/// <param name="assembly">The <see cref="InsAssembly"/> to format.</param>
	/// <returns>A non-null formatted assembly reference string.</returns>
	/// <exception cref="ArgumentNullException"><paramref name="assembly"/> is <see langword="null"/>.</exception>
	public static string Format(InsAssembly assembly)
	{
		ArgumentNullException.ThrowIfNull(assembly);

		if (assembly.TryFastFormat(out var result))
		{
			return result;
		}

		return Write(BuilderPool.Rent(), assembly).ToStringAndReturn();
	}

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
		{
			type.ElementType.Apply(this, builder).Append('[');

			if (type.Rank == 1)
			{
				builder.Append('*');
			}
			else
			{
				builder.Append(',', type.Rank - 1);
			}

			return builder.Append(']');
		}

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

		public StringBuilder VisitSZArray(InsSZArrayType type, StringBuilder argument)
			=> type.ElementType.Apply(this, argument).Append("[]");

		public static StringBuilder WriteAssembly(InsAssembly assembly, StringBuilder builder)
		{
			WriteIdentifier(assembly.Name, builder);

			foreach (var qualification in assembly.Qualifications)
			{
				builder.Append(", ");
				WriteIdentifier(qualification.Name, builder);
				builder.Append('=');

				if (RequiresQuoting(qualification.Value))
				{
					WriteQuotedIdentifier(qualification.Value, builder);
				}
				else
				{
					WriteIdentifier(qualification.Value, builder);
				}
			}

			return builder;
		}

		static void WriteIdentifier(string identifier, StringBuilder builder)
		{
			WriteWithEscapedDelimiters(builder, identifier, Delimiters.All);
		}

		static void WriteQuotedIdentifier(string identifier, StringBuilder builder)
		{
			builder.Append('"');
			WriteWithEscapedDelimiters(builder, identifier, Delimiters.Quote);
			builder.Append('"');
		}

#if NET
		static void WriteWithEscapedDelimiters(StringBuilder builder, string identifier, SearchValues<char> delimiters)
#else
		static void WriteWithEscapedDelimiters(StringBuilder builder, string identifier, ReadOnlySpan<char> delimiters)
#endif
		{
			var start = 0;

			while (start < identifier.Length)
			{
				var remaining = identifier.AsSpan(start);

				var index = remaining.IndexOfAny(delimiters);

				if (index < 0)
				{
					builder.Append(remaining);
					return;
				}

				builder
					.Append(remaining.Slice(0, index))
					.Append('\\')
					.Append(remaining[index]);

				start = start + index + 1;
			}
		}

		static bool RequiresQuoting(string identifier)
			=> string.IsNullOrEmpty(identifier) || identifier.AsSpan().ContainsAny(Delimiters.All);
	}

	sealed class AssemblyLocator : IInsTypeVisitor<object, InsAssembly?>
	{
		public static AssemblyLocator Instance { get; } = new AssemblyLocator();

		AssemblyLocator()
		{
		}

		public InsAssembly? VisitArray(InsArrayType type, object argument) => type.ElementType.Apply(this, argument);
		public InsAssembly? VisitByRef(InsByRefType type, object argument) => type.ElementType.Apply(this, argument);
		public InsAssembly? VisitGeneric(InsGenericType type, object argument) => AssemblyFromNamed(type.Definition);
		public InsAssembly? VisitNamed(InsNamedType type, object argument) => AssemblyFromNamed(type);
		public InsAssembly? VisitPointer(InsPointerType type, object argument) => type.ElementType.Apply(this, argument);
		public InsAssembly? VisitSZArray(InsSZArrayType type, object argument) => type.ElementType.Apply(this, argument);

		static InsAssembly? AssemblyFromNamed(InsNamedType type)
		{
			while (type.DeclaringType != null)
			{
				type = type.DeclaringType;
			}

			return type.Assembly;
		}
	}
}
