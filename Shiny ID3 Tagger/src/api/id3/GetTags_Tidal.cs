//-----------------------------------------------------------------------
// <copyright file="GetTags_Tidal.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Gets ID3 data from Tidal API for current track</summary>
// https://github.com/lucaslg26/TidalAPI/blob/master/lib/client.js
// https://pythonhosted.org/tidalapi/api.html#api
//-----------------------------------------------------------------------

namespace GetTags
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Net;
    using System.Net.Http;
    using System.Threading;
    using System.Threading.Tasks;
    using GlobalVariables;
    using Newtonsoft.Json.Linq;
    using Utils;

    public class Tidal : IGetTagsService
    {
        public const string ServiceName = "Tidal";

        public async Task<Id3> GetTags(HttpMessageInvoker client, string artist, string title,
            CancellationToken cancelToken)
        {
            Id3 o = new Id3 {Service = ServiceName};

            Stopwatch sw = new Stopwatch();
            sw.Start();

            // ###########################################################################
            string searchTermEnc = WebUtility.UrlEncode(artist + " - " + title);

            if (ApiSessionData.TiSessionID == null || ApiSessionData.TiSessionExpireDate < DateTime.Now)
            {
                using (HttpRequestMessage loginRequest = new HttpRequestMessage())
                {
                    loginRequest.Method = HttpMethod.Post;
                    loginRequest.RequestUri = new Uri("http://api.tidalhifi.com/v1/login/username");
                    loginRequest.Headers.Add("X-Tidal-Token", (string)User.Accounts["Tidal"]["Token"]);
                    loginRequest.Content = new FormUrlEncodedContent(new[]
                    {
                        new KeyValuePair<string, string>("username", (string)User.Accounts["Tidal"]["Username"]),
                        new KeyValuePair<string, string>("password", (string)User.Accounts["Tidal"]["Password"])
                    });

                    string loginContent = await Utils.GetResponse(client, loginRequest, cancelToken);
                    JObject loginData = Utils.DeserializeJson(loginContent);

                    if (loginData?.SelectToken("sessionId") != null)
                    {
                        ApiSessionData.TiSessionID = (string)loginData.SelectToken("sessionId");
                        ApiSessionData.TiCountryCode = (string)loginData.SelectToken("countryCode");

                        string userID = (string)loginData.SelectToken("userId");

                        using (HttpRequestMessage sessionRequest = new HttpRequestMessage())
                        {
                            sessionRequest.RequestUri =
                                new Uri("http://api.tidalhifi.com/v1/users/" + userID + "/subscription");
                            sessionRequest.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);

                            string sessionContent = await Utils.GetResponse(client, sessionRequest, cancelToken);
                            JObject sessionData = Utils.DeserializeJson(sessionContent);

                            if (sessionData != null)
                            {
                                // 30mins is the "offlineGracePeriod" which I assume is the timespan a session is valid. I could be wrong since there is no documentation about this
                                TimeSpan validDuration =
                                    TimeSpan.FromSeconds(
                                        (int)sessionData.SelectToken("subscription.offlineGracePeriod") * 60);
                                ApiSessionData.TiSessionExpireDate = DateTime.Now.Add(validDuration);
                            }
                        }
                    }
                }
            }

            if (ApiSessionData.TiSessionID != null)
            {
                using (HttpRequestMessage searchRequest = new HttpRequestMessage())
                {
                    searchRequest.Headers.Add("Origin", "http://listen.tidal.com");
                    searchRequest.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);
                    searchRequest.RequestUri = new Uri("http://api.tidalhifi.com/v1/search?types=TRACKS&countryCode=" +
                                                       ApiSessionData.TiCountryCode + "&query=" + searchTermEnc);

                    string searchContent = await Utils.GetResponse(client, searchRequest, cancelToken);
                    JObject searchData = Utils.DeserializeJson(searchContent);

                    if (searchData?.SelectToken("tracks.items[0]") != null)
                    {
                        o.Artist = (string) searchData.SelectToken("tracks.items[0].artists[0].name");
                        o.Title = (string) searchData.SelectToken("tracks.items[0].title");
                        o.Album = (string) searchData.SelectToken("tracks.items[0].album.title");
                        o.TrackNumber = (string) searchData.SelectToken("tracks.items[0].trackNumber");
                        o.DiscNumber = (string) searchData.SelectToken("tracks.items[0].volumeNumber");

                        if (searchData.SelectToken("tracks.items[0].album.id") != null)
                        {
                            HttpRequestMessage albumRequest = new HttpRequestMessage();
                            albumRequest.Headers.Add("Origin", "http://listen.tidal.com");
                            albumRequest.Headers.Add("X-Tidal-SessionId", ApiSessionData.TiSessionID);
                            albumRequest.RequestUri =
                                new Uri("http://api.tidalhifi.com/v1/albums/" +
                                        (string) searchData.SelectToken("tracks.items[0].album.id") + "?countryCode=" +
                                        ApiSessionData.TiCountryCode);

                            string albumContent = await Utils.GetResponse(client, albumRequest, cancelToken);
                            JObject albumData = Utils.DeserializeJson(albumContent);

                            if (albumData != null)
                            {
                                o.Genre =
                                    null; // tidal API doesn't provide genres for specific items, only a general list of genres (https://pythonhosted.org/tidalapi/api.html#api)
                                o.Date = (string) albumData.SelectToken("releaseDate");
                                o.TrackCount = (string) albumData.SelectToken("numberOfTracks");
                                o.DiscCount = (string) albumData.SelectToken("numberOfVolumes");

                                if (albumData.SelectToken("cover") != null)
                                {
                                    o.Cover = "http://resources.tidal.com/images/" +
                                              ((string) albumData.SelectToken("cover")).Replace("-", "/") +
                                              "/1280x1280.jpg";
                                }
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