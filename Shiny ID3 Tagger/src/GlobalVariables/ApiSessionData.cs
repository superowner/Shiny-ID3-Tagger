//-----------------------------------------------------------------------
// <copyright file="ApiSessionData.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GlobalVariables
{
	using System;

	/// <summary>
	/// Represents a class for global API variables which are set during program run.
	/// For example to remember if a certain login token is still valid if multiple requests are made
	/// </summary>
	internal static class ApiSessionData
	{
		/// <summary>
		/// Gets or sets the Microsoft Groove access token
		/// </summary>
		internal static string MsAccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Microsoft Groove access token expire date
		/// </summary>
		internal static DateTime MsAccessTokenExpireDate { get; set; }

		/// <summary>
		/// Gets or sets the Spotify access token
		/// </summary>
		internal static string SpAccessToken { get; set; }

		/// <summary>
		/// Gets or sets the Spotify access token expire date
		/// </summary>
		internal static DateTime SpAccessTokenExpireDate { get; set; }

		/// <summary>
		/// Gets or sets the Tidal session ID
		/// </summary>
		internal static string TiSessionID { get; set; }

		/// <summary>
		/// Gets or sets the Tidal country code
		/// </summary>
		internal static string TiCountryCode { get; set; }

		/// <summary>
		/// Gets or sets the Tidal session expire date
		/// </summary>
		internal static DateTime TiSessionExpireDate { get; set; }
	}
}
