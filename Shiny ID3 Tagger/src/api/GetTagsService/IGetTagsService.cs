//-----------------------------------------------------------------------
// <copyright file="IGetTagsService.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace GetTags
{
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;

	/// <summary>
	/// Provides the common interface for all ID3 API services
	/// </summary>
	internal interface IGetTagsService
	{
		/// <summary>
		/// Provides the common interface for all ID3 API services
		/// </summary>
		/// <param name="client">The HTTP client which is passed on to GetResponse method</param>
		/// <param name="artist">The input artist to search for</param>
		/// <param name="title">The input song title to search for</param>
		/// <param name="cancelToken">The cancelation token which is passed on to GetResponse method</param>
		/// <returns>The ID3 tag object with the results from this API</returns>
		Task<Id3> GetTags(
			HttpMessageInvoker client,
			string artist,
			string title,
			CancellationToken cancelToken);
	}
}
