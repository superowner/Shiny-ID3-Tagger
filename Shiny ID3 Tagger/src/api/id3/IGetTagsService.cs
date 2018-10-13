//-----------------------------------------------------------------------
// <copyright file="IGetTagsService.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Provides the common interface for all ID3 API services</summary>
//-----------------------------------------------------------------------

namespace GetTags
{
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using GlobalVariables;

    internal interface IGetTagsService
    {
        Task<Id3> GetTags(
            HttpMessageInvoker client,
            string artist,
            string title,
            CancellationToken cancelToken);
    }
}
