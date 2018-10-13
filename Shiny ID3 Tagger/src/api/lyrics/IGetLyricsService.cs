//-----------------------------------------------------------------------
// <copyright file="IGetLyricsService.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Provides the common interface for all ID3 lyrics services</summary>
//-----------------------------------------------------------------------

namespace GetLyrics
{
	using System.Net.Http;
	using System.Threading;
	using System.Threading.Tasks;
	using GlobalVariables;

	internal interface IGetLyricsService
	{
		Task<Id3> GetLyrics(HttpMessageInvoker client, Id3 tagNew, CancellationToken cancelToken);
	}
}
