//-----------------------------------------------------------------------
// <copyright file="GetTags_Amazon.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Amazon API for current track</summary>
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/prod-adv-api-dg.pdf
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/APPNDX_SortValuesArticle.html
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/rest-signature.html
// https://docs.aws.amazon.com/AWSECommerceService/latest/DG/LocaleUS.html
// https://stackoverflow.com/questions/12097650/use-powershell-to-get-book-metadata-from-amazon
// in addition to .NET 4.0 EscapeDataString		forbidden:	!'()*
// in addition to .NET 4.0 EscapeDataString		allowed:	-_.~
// replace after EscapeDataString				!=%21	'=%27	(=%28	)=%29	*=%2A
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Diagnostics;
	using System.Linq;
	using System.Net.Http;	
	using System.Security.Cryptography;
	using System.Text;
	using System.Threading;
	using System.Threading.Tasks;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;

	public partial class Form1
	{
		// ###########################################################################
		private static string CreateSignature(string server, string parameters)
		{
			string stringToSign = "GET" + "\n" + server + "\n" + "/onca/xml" + "\n" + parameters;		
			byte[] bytesToSign = Encoding.UTF8.GetBytes(stringToSign);
			HMACSHA256 hmacSha = new HMACSHA256();
			hmacSha.Key = Encoding.UTF8.GetBytes(User.Accounts["AmSecretKey"]);
			byte[] sigBytes = hmacSha.ComputeHash(bytesToSign);
			string sigBase64 = Convert.ToBase64String(sigBytes);
			string sigEncoded = Uri.EscapeDataString(sigBase64);
			return sigEncoded;
		}
		
		private async Task<Id3> GetTags_Amazon(HttpMessageInvoker client, string artist, string title, CancellationToken cancelToken)
		{
			Id3 o = new Id3();
			o.Service = "Amazon";

			Stopwatch sw = new Stopwatch();
			sw.Start();
			
			// ###########################################################################
			const string Server = "webservices.amazon.com";
			
			string timestamp = DateTime.UtcNow.ToString("yyyy-MM-ddTHH:mm:ssZ");
			timestamp = Uri.EscapeDataString(timestamp);
			
			string artistEnc = Uri.EscapeDataString(artist).Replace(@"!", "%21").Replace(@"'", "%27").Replace(@"(", "%28").Replace(@")", "%29").Replace(@"*", "%2A");
			string titleEnc = Uri.EscapeDataString(title).Replace(@"!", "%21").Replace(@"'", "%27").Replace(@"(", "%28").Replace(@")", "%29").Replace(@"*", "%2A");
			
			string parameters = "AWSAccessKeyId=" + User.Accounts["AmAccessKey"] +
								"&AssociateTag=" + User.Accounts["AmAssociateTag"] +
								"&Keywords=" + artistEnc +
								"&Operation=ItemSearch" +
								"&ResponseGroup=ItemAttributes" +
								"&SearchIndex=MP3Downloads" +
								"&Service=AWSECommerceService" +
								"&Timestamp=" + timestamp +
								"&Title=" + titleEnc +
								"&Version=2013-08-01";
			
			HttpRequestMessage request = new HttpRequestMessage();
			request.RequestUri = new Uri("https://" + Server + "/onca/xml?" + parameters + "&Signature=" + CreateSignature(Server, parameters));

			string content1 = await this.GetRequest(client, request, cancelToken);
			JObject data1 = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content1), this.GetJsonSettings());

			if (data1 != null && data1.SelectToken("ItemSearchResponse.Items.Item") != null)
			{
				JToken item = null;
				if (data1.SelectToken("ItemSearchResponse.Items.Item") != null)
				{
					if (data1.SelectToken("ItemSearchResponse.Items.Item").Type == JTokenType.Array)
					{
						item = data1.SelectToken("ItemSearchResponse.Items.Item[0]");
					}
					else
					{
						item = data1.SelectToken("ItemSearchResponse.Items.Item");
					}
				}
				
				o.Artist = (string)item.SelectToken("ItemAttributes.Creator.#text");
				o.Title = (string)item.SelectToken("ItemAttributes.Title");
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

				string asin = (string)item.SelectToken("ASIN");

				// ###########################################################################
				parameters = "AWSAccessKeyId=" + User.Accounts["AmAccessKey"] +
							"&AssociateTag=" + User.Accounts["AmAssociateTag"] +
							"&Condition=All" +
							"&IdType=ASIN" +
							"&ItemId=" + asin +
							"&Operation=ItemLookup" +
							"&RelationshipType=Tracks" +
							"&ResponseGroup=RelatedItems" +
							"&Service=AWSECommerceService" +
							"&Timestamp=" + timestamp +
							"&Version=2013-08-01";

				request = new HttpRequestMessage();
				request.RequestUri = new Uri("https://" + Server + "/onca/xml?" + parameters + "&Signature=" + CreateSignature(Server, parameters));

				string content2 = await this.GetRequest(client, request, cancelToken);
				JObject data2 = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content2), this.GetJsonSettings());

				if (data2 != null && data2.SelectToken("ItemLookupResponse.Items.Item.RelatedItems.RelatedItem.Item.ASIN") != null)
				{
					asin = (string)data2.SelectToken("ItemLookupResponse.Items.Item.RelatedItems.RelatedItem.Item.ASIN");

					parameters = "AWSAccessKeyId=" + User.Accounts["AmAccessKey"] +
								"&AssociateTag=" + User.Accounts["AmAssociateTag"] +
								"&Condition=All" +
								"&IdType=ASIN" +
								"&ItemId=" + asin +
								"&Operation=ItemLookup" +
								"&RelationshipType=Tracks" +
								"&ResponseGroup=RelatedItems%2CLarge" +						
								"&Service=AWSECommerceService" +
								"&Timestamp=" + timestamp +
								"&Version=2013-08-01";
	
					request = new HttpRequestMessage();
					request.RequestUri = new Uri("https://" + Server + "/onca/xml?" + parameters + "&Signature=" + CreateSignature(Server, parameters));
	
					string content3 = await this.GetRequest(client, request, cancelToken);
					JObject data3 = JsonConvert.DeserializeObject<JObject>(this.ConvertXmlToJson(content3), this.GetJsonSettings());
					
					if (data3 != null)
					{
						o.TrackCount = (string)data3.SelectToken("ItemLookupResponse.Items.Item.RelatedItems.RelatedItemCount");
						o.Album = (string)data3.SelectToken("ItemLookupResponse.Items.Item.ItemAttributes.Title");
						o.Cover = (string)data3.SelectToken("ItemLookupResponse.Items.Item.ImageSets.ImageSet.HiResImage.URL");						
					}
				}
			}

			// ###########################################################################
			sw.Stop();
			o.Duration = string.Format("{0:s\\,f}", sw.Elapsed);

			request.Dispose();
			return o;
		}
	}
}

// System.IO.File.WriteAllText (@"D:\response.json", content2);