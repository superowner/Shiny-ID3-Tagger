//-----------------------------------------------------------------------
// <copyright file="GetTags_Amazon.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Amazon API for current track</summary>
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/prod-adv-api-dg.pdf
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/rest-signature.html
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/LocaleUS.html
// https://webservices.amazon.com/scratchpad/index.html
// in addition to .NET 4.0 EscapeDataString		forbidden:	!'()*
// in addition to .NET 4.0 EscapeDataString		allowed:	-_.~
// replace after EscapeDataString				!=%21	'=%27	(=%28	)=%29	*=%2A
//-----------------------------------------------------------------------

namespace GetTags
{
    using System;
    using System.Diagnostics;
    using System.Net.Http;
    using System.Security.Cryptography;
    using System.Text;
    using System.Threading;
    using System.Threading.Tasks;
    using GlobalVariables;
    using Newtonsoft.Json.Linq;
    using Utils;

	[Obsolete("Amazon key is not registered as an Amazon Associate. ",true)]
	public class Amazon : IGetTagsService
	{
		private static Stopwatch lastRequestTimer = new Stopwatch();
		private static int lastRequestTimeout = 1000;
		public const string ServiceName = "Amazon";

		// ###########################################################################
		private static string CreateSignature(string server, string parameters)
		{
			string stringToSign = "GET" + "\n" + server + "\n" + "/onca/xml" + "\n" + parameters;
			byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
			HMACSHA256 hmacSha = new HMACSHA256();
			hmacSha.Key = Encoding.UTF8.GetBytes((string)User.Accounts["Amazon"]["SecretKey"]);
			byte[] sigBytes = hmacSha.ComputeHash(bytesToSign);
			string sigBase64 = Convert.ToBase64String(sigBytes);
			string sigEncoded = Uri.EscapeDataString(sigBase64);
			return sigEncoded;
		}

		public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3 {Service = ServiceName};

			Stopwatch sw = new Stopwatch();
			sw.Start();

			// ###########################################################################
			const string Server = "webservices.amazon.com";

			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			timestamp = Uri.EscapeDataString(timestamp);

			// Replace is needed according to official C# example for product advertising API
			string artistEncoded = Uri.EscapeDataString(artist).Replace(@"!", "%21").Replace(@"'", "%27").Replace(@"(", "%28").Replace(@")", "%29").Replace(@"*", "%2A");
			string titleEncoded = Uri.EscapeDataString(title).Replace(@"!", "%21").Replace(@"'", "%27").Replace(@"(", "%28").Replace(@")", "%29").Replace(@"*", "%2A");

			// Initial search
			string parameters = "AWSAccessKeyId=" + User.Accounts["Amazon"]["AccessKey"] +
								"&AssociateTag=" + User.Accounts["Amazon"]["AssociateTag"] +
								"&Keywords=" + artistEncoded +
								"&Operation=ItemSearch" +
								"&RelationshipType=Tracks" +
								"&ResponseGroup=ItemAttributes%2CRelatedItems" +
								"&SearchIndex=MP3Downloads" +
								"&Service=AWSECommerceService" +
								"&Timestamp=" + timestamp +
								"&Title=" + titleEncoded +
								"&Version=2013-08-01";

			using (HttpRequestMessage searchRequest = new HttpRequestMessage())
			{
				searchRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
				searchRequest.RequestUri = new Uri("http://" + Server + "/onca/xml?" + parameters + "&Signature=" + CreateSignature(Server, parameters));

				// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/TroubleshootingApplications.html#efficiency-guidelines
				// Check if 1 second already passed since last request
				while (lastRequestTimer.ElapsedMilliseconds < lastRequestTimeout && lastRequestTimer.IsRunning)
				{
					await Task.Delay(50);
				}

				string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
				lastRequestTimer.Restart();

				JObject searchData = Utils.DeserializeJson(Utils.ConvertXmlToJson(searchContent));

				if (searchData != null && searchData.SelectToken("ItemSearchResponse.Items.Item") != null)
				{
					JToken item = null;
					if (searchData.SelectToken("ItemSearchResponse.Items.Item") != null)
					{
						if (searchData.SelectToken("ItemSearchResponse.Items.Item").Type == JTokenType.Array)
						{
							item = searchData.SelectToken("ItemSearchResponse.Items.Item[0]");
						}
						else
						{
							item = searchData.SelectToken("ItemSearchResponse.Items.Item");
						}
					}

					o.Artist = (string)item.SelectToken("ItemAttributes.Creator.#text");
					o.Title = (string)item.SelectToken("ItemAttributes.Title");
					o.Album = (string)item.SelectToken("RelatedItems.RelatedItem.Item.ItemAttributes.Title");
					o.Date = (string)item.SelectToken("ItemAttributes.PublicationDate");
					o.TrackNumber = (string)item.SelectToken("ItemAttributes.TrackSequence");
					o.DiscCount = null;
					o.DiscNumber = null;

					o.Genre = (string)item.SelectToken("ItemAttributes.Genre");
					if (o.Genre != null)
					{
						o.Genre = o.Genre.Replace("-music", string.Empty);
						o.Genre = o.Genre.Replace("-", " ");
					}

					// ###########################################################################
					// Get related items from album (this shows up all tracks on the album, add 'large' as response-group to get cover links)
					string asin = (string)item.SelectToken("RelatedItems.RelatedItem.Item.ASIN");
					if (asin != null)
					{
						parameters = "AWSAccessKeyId=" + User.Accounts["Amazon"]["AccessKey"] +
									"&AssociateTag=" + User.Accounts["Amazon"]["AssociateTag"] +
									"&Condition=All" +
									"&IdType=ASIN" +
									"&ItemId=" + asin +
									"&Operation=ItemLookup" +
									"&RelationshipType=Tracks" +
									"&ResponseGroup=RelatedItems%2CLarge" +
									"&Service=AWSECommerceService" +
									"&Timestamp=" + timestamp +
									"&Version=2013-08-01";

						using (HttpRequestMessage albumRequest = new HttpRequestMessage())
						{
							albumRequest.Headers.Add("User-Agent", (string)User.Settings["UserAgent"]);
							albumRequest.RequestUri = new Uri("http://" + Server + "/onca/xml?" + parameters + "&Signature=" + CreateSignature(Server, parameters));

							// Check if 1 second already passed since last request
							while (lastRequestTimer.ElapsedMilliseconds < lastRequestTimeout && lastRequestTimer.IsRunning)
							{
								await Task.Delay(50);
							}

							string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
							lastRequestTimer.Restart();

							JObject albumData = Utils.DeserializeJson(Utils.ConvertXmlToJson(albumContent));

							if (albumData != null)
							{
								o.TrackCount = (string)albumData.SelectToken("ItemLookupResponse.Items.Item.RelatedItems.RelatedItemCount");
								o.Cover = (string)albumData.SelectToken("ItemLookupResponse.Items.Item.ImageSets.ImageSet.HiResImage.URL");
							}
						}
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = $"{sw.Elapsed:s\\,f}";

			return o;
		}
	}
}
