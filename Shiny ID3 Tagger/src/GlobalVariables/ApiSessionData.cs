//-----------------------------------------------------------------------
// <copyright file="ApiSessionData.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Static or global variables</summary>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;

	internal static class ApiSessionData
	{
		internal static string MsAccessToken { get; set; }

		internal static DateTime MsAccessTokenExpireDate { get; set; }

		internal static string SpAccessToken { get; set; }

		internal static DateTime SpAccessTokenExpireDate { get; set; }

		internal static string TiSessionID { get; set; }

		internal static string TiCountryCode { get; set; }

		internal static DateTime TiSessionExpireDate { get; set; }
	}
}
