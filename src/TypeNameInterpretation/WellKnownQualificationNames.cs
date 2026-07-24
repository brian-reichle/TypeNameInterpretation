// Copyright (c) Brian Reichle.  All Rights Reserved.  Licensed under the MIT License.  See LICENSE in the project root for license information.
namespace TypeNameInterpretation;

/// <summary>
/// Contains standard key names for assembly qualifications.
/// </summary>
public static class WellKnownQualificationNames
{
	/// <summary>
	/// The qualification key name for the assembly version ("Version").
	/// </summary>
	public const string Version = "Version";

	/// <summary>
	/// The qualification key name for the full public key ("PublicKey").
	/// </summary>
	public const string PublicKey = "PublicKey";

	/// <summary>
	/// The qualification key name for the public key token ("PublicKeyToken").
	/// </summary>
	public const string PublicKeyToken = "PublicKeyToken";

	/// <summary>
	/// The qualification key name for the assembly culture ("Culture").
	/// </summary>
	public const string Culture = "Culture";

	/// <summary>
	/// The qualification key name for the target processor architecture ("ProcessorArchitecture").
	/// </summary>
	public const string ProcessorArchitecture = "ProcessorArchitecture";
}
