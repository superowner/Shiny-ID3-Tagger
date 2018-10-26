//-----------------------------------------------------------------------
// <copyright file="User.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;
	using Newtonsoft.Json.Linq;

	/// <summary>
	/// Represents a class to hold all user settings and API accounts like usernames, passwords or tokens
	/// </summary>
	internal static class User
	{
		/// <summary>
		/// Gets or sets user accounts to login, authorize and use API services
		/// </summary>
		internal static JObject Accounts { get; set; }

		/// <summary>
		/// Gets or sets program settings which the user is allowed to change
		/// </summary>
		internal static JObject Settings { get; set; }
	}
}
