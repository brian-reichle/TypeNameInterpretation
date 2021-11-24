// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
using System;

namespace TypeNameInterpretation
{
	public abstract class InsElementedType : InsType
	{
		private protected InsElementedType(InsType elementType)
		{
			ElementType = elementType ?? throw new ArgumentNullException(nameof(elementType));
		}

		public InsType ElementType { get; }
	}
}
