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

	internal static class User
	{
		internal static JObject Accounts { get; set; }

		internal static JObject Settings { get; set; }
	}
}
