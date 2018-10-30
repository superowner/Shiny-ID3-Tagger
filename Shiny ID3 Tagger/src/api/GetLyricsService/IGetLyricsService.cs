//-----------------------------------------------------------------------
// <copyright file="IGetLyricsService.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetLyrics
{
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;

	/// <summary>
	/// Provides the common interface for all ID3 lyrics services
	/// </summary>
	internal interface IGetLyricsService
	{
		/// <summary>
		/// Provides the common interface for all ID3 lyrics services
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="tagNew">The input artist and song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>The ID3 tag object with the results from this API for lyrics</returns>
		Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken);
	}
}
