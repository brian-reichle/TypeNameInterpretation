// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.

#if !NET
namespace System.Runtime.CompilerServices;

[AttributeUsage(AttributeTargets.Parameter)]
sealed class CallerArgumentExpressionAttribute(string parameterName) : Attribute
{
	public string ParameterName { get; } = parameterName;
}
#endif
