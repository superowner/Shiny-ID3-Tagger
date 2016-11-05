//-----------------------------------------------------------------------
// <copyright file="GlobalMethods.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Collection of globally used methods by ExternalAPI tasks</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.Diagnostics;
	using System.Drawing;
	using System.Globalization;
	using System.IO;
	using System.Linq;
	using System.Net;
	using System.Net.Http;
	using System.Net.NetworkInformation;
	using System.Security.Cryptography;
	using System.Text;
	using System.Text.RegularExpressions;
	using System.Threading;
	using System.Threading.Tasks;
	using System.Xml;
	using Newtonsoft.Json;

	public partial class Form1
	{
		// ###########################################################################
		private static string Capitalize(string str)
		{
			// This method adheres rule #1, rule #2 and rule #4 from http://aitech.ac.jp/~ckelly/midi/help/caps.html
			if (!string.IsNullOrWhiteSpace(str))
			{
				string[] lowercase =
				{
					"a", "an", "the", "and", "but", "or", "nor", "at", "by", "for", "from", "in",
					"into", "of", "off", "on", "onto", "out", "over", "to", "up", "with", "as"
				};

				// Use regex to extract all words from str. Use custom word seperators
				// Edge case "P!nk" is not split into two words since the exclamation mark is not included in seperator list
				// Edge case "Mind the G.A.T.T" is not split into several words since the point is not included in seperator list
				const string Sep = @" ,&/\(\)\{\}\[\]\><";
				MatchCollection words = Regex.Matches(str, "(?<=^|([" + Sep + "])+)[^" + Sep + "]+?(?=$|([" + Sep + "])+)");

				int firstIndex = words[0].Index;
				int lastIndex = words[words.Count - 1].Index;

				foreach (Match w in words)
				{
					// ALL CAPS words should stay in uppercase. Except the "A"
					string word = w.Value;
					if (word == word.ToUpperInvariant() && word != "A")
					{
						continue;
					}

					// Search special words and lowercase them. But ignore the first/last word. They must be capitalized
					// "and" => stays "and"			"And" => goes "and"		"AND" => stays "AND"
					if (lowercase.Contains(word.ToLower(Runtime.CultEng)) && w.Index != firstIndex && w.Index != lastIndex)
					{
						string newWord = word.ToLower(Runtime.CultEng);
						str = str.Remove(w.Index, w.Length).Insert(w.Index, newWord);
						continue;
					}

					// All remaining lowercase only words are capitalized
					// edge cases like "iTunes" or "CHURCHES" stay as they are since they are not "lowercase only"
					if (word == word.ToLower(Runtime.CultEng))
					{
						string newWord = word.First().ToString().ToUpperInvariant() + word.Substring(1);
						str = str.Remove(w.Index, w.Length).Insert(w.Index, newWord);
					}
				}
			}

			return str;
		}

		// ###########################################################################
		private static string Strip(string str)
		{
			if (str != null)
			{
				if (User.Settings["RemoveBrackets"])
				{
					const string RegExPattern = @"[\(\[\{].*?[\)\]\}]";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				if (User.Settings["RemoveFeaturing"])
				{
					const string RegExPattern = @" ([\(\[\{])?(feat(\.)?|ft(\.)?|featuring)(.*?[\)\]\}]|.*)";
					str = Regex.Replace(str, RegExPattern, string.Empty, RegexOptions.IgnoreCase);
				}

				str = Regex.Replace(str, @"[‘’ʼ]", "'");
				str = Regex.Replace(str, @"[‐‑−－]", "-");		// https://www.cs.tut.fi/~jkorpela/dashes.html
				str = str.Trim();
			}

			return str;
		}

		// ###########################################################################
		private static string WellFormedCsvValue(object cell)
		{
			string value = cell != null ? cell.ToString() : string.Empty;
			value = value.Replace("\r\n", "\n");
			value = value.Replace("\"", "\"\"");
			value = "\"" + value + "\"";
			return value;
		}

		// ###########################################################################
		private static DateTime ConvertStringToDate(string dateString)
		{
			// Valid formats according to http://id3.org/id3v2.4.0-structure
			string[] formats =
			{
				"yyyy",
				"yyyy-MM",
				"yyyy-MM-dd",
				"yyyy-MM-dd HH",
				"yyyy-MM-dd HH:mm",
				"yyyy-MM-dd HH:mm:ss",
				"dd/yyyy",
				"MM/dd/yyyy",
				"MM/dd/yyyy HH",
				"MM/dd/yyyy HH:mm",
				"MM/dd/yyyy HH:mm:ss"
			};

			DateTime resultDate = new DateTime();
			if (DateTime.TryParseExact(dateString, formats, Runtime.CultEng, DateTimeStyles.None, out resultDate))
			{
				return resultDate;
			}
			else
			{
				Debug.WriteLine(dateString);
			}

			return default(DateTime);
		}

		// ###########################################################################
		private static bool IsValidUrl(string stringUri)
		{
			Uri uriResult;
			bool result = Uri.TryCreate(stringUri, UriKind.Absolute, out uriResult)
								&& (uriResult.Scheme == Uri.UriSchemeHttp
								|| uriResult.Scheme == Uri.UriSchemeHttps);
			return result;
		}

		// ###########################################################################
		private static HttpClient InitiateHttpClient()
		{
			HttpClientHandler handler = new HttpClientHandler();

			Ping ping = new Ping();

			try
			{
				PingReply reply = ping.Send(User.Settings["Proxy"].Split(':')[0], 100);

				if (reply.Status == IPStatus.Success)
				{
					handler.Proxy = new WebProxy(User.Settings["Proxy"], false);
					handler.UseProxy = true;
				}
			}
			catch (ArgumentException)
			{
				// user entered an invalid "proxy:port" string in settings.json e.g. "0.0.0.0:0000"
			}
			catch (NullReferenceException)
			{
				// User closed the window while ping was still running
			}

			HttpClient client = new HttpClient(handler);
			client.MaxResponseContentBufferSize = 256000000;
			client.DefaultRequestHeaders.TryAddWithoutValidation("Content-Type", "application/json; charset=utf-8");
			client.DefaultRequestHeaders.Add("User-Agent", User.Settings["UserAgent"]);

			ping.Dispose();
			return client;
		}

		// ###########################################################################
		private static HttpRequestMessage CloneRequest(HttpRequestMessage original)
		{
			HttpRequestMessage backup = new HttpRequestMessage(original.Method, original.RequestUri);

			backup.Content = original.Content;
			backup.Version = original.Version;

			foreach (KeyValuePair<string, object> property in original.Properties)
			{
				backup.Properties.Add(property);
			}

			foreach (KeyValuePair<string, IEnumerable<string>> header in original.Headers)
			{
				backup.Headers.TryAddWithoutValidation(header.Key, header.Value);
			}

			return backup;
		}

		// ###########################################################################
		private async Task<string> GetRequest(HttpMessageInvoker client, HttpRequestMessage request, CancellationToken cancelToken)
		{
			const int Timeout = 10;
			const int MaxRetries = 3;

			HttpResponseMessage response = new HttpResponseMessage();
			HttpRequestMessage requestBackup = CloneRequest(request);

			string responseString = string.Empty;

			for (int i = MaxRetries; i >= 1; i--)
			{
				if (cancelToken.IsCancellationRequested)
				{
					return string.Empty;
				}

				request = CloneRequest(requestBackup);

				var timeoutToken = CancellationTokenSource.CreateLinkedTokenSource(cancelToken);
				timeoutToken.CancelAfter(TimeSpan.FromSeconds(Timeout));

				// TODO: skip a webservice entirely if it wasn't reachable three times
				try
				{
					response = await client.SendAsync(request, timeoutToken.Token);

					// These are common errors. We suppress them and dont show an error message. Don't read body. Return with empty string
					if ((request.RequestUri.Host == "api.spotify.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "music.xboxlive.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "coverartarchive.org" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.lololyrics.com" && response.StatusCode == HttpStatusCode.NotFound) ||
						(request.RequestUri.Host == "api.musicgraph.com" && response.StatusCode == HttpStatusCode.InternalServerError) ||
						(request.RequestUri.Host == "api.chartlyrics.com" && response.StatusCode == HttpStatusCode.InternalServerError) ||
						(request.RequestUri.Host == "data.quantonemusic.com" && response.StatusCode == HttpStatusCode.Forbidden))
					{
						break;
					}

					if (response.IsSuccessStatusCode)
					{
						// response was successful. Read body and return
						responseString = await response.Content.ReadAsStringAsync();
						this.Log("network", new[] { request.RequestUri.Scheme + "://" + request.RequestUri.Authority});
						break;
					}
					else
					{
						// response was not successful. But it was also not a common error. Try to parse HTML and show it to the user via GUI
						string errorResponse = response.Content.ReadAsStringAsync().Result;

						Match match = Regex.Match(errorResponse, "(?<=<body>)(?<text>.*?)(?=</body>)", RegexOptions.Singleline);
						if (match.Success)
						{
							errorResponse = match.Groups["text"].Value.Trim();
						}

						// If HTML parsing failed, show the complete response including HTML tags
						string[] errorMsg =
							{
								"ERROR: Server response was unsuccessful",
								"Response code: " + response.ReasonPhrase + ": " + (int)response.StatusCode,
								"Requst URL: " + request.RequestUri.OriginalString,
								"Response message: " + errorResponse,
								"Retries left: " + i + "/" + MaxRetries
							};
						this.Log("error", errorMsg);
					}
				}
				catch (TaskCanceledException)
				{
					// The request timed out or was canceled by user
					if (!cancelToken.IsCancellationRequested)
					{
						string[] errorMsg =
						{
							"ERROR: Server response took longer than " + Timeout + " seconds",
							"Requst URL: " + request.RequestUri.OriginalString
						};
						this.Log("error", errorMsg);
					}

					break;
				}
				catch (Exception ex)
				{
					// An unknown application error occured. Catch it and redirect it to debug console
					string[] errorMsg =
					{
						"ERROR: Unknown error occured",
						"Requst URL: " + request.RequestUri.OriginalString,
						"Retries left: " + i + "/" + MaxRetries
					};
					this.Log("error", errorMsg);

					Debug.WriteLine(errorMsg);
					Debug.WriteLine(ex);

					break;
				}

				await Task.Delay(2000);
			}

			response.Dispose();
			requestBackup.Dispose();

			return responseString;
		}

		// ###########################################################################
		private bool ReadUserVariables()
		{
			try
			{
				// The key for decryption is hardcoded on purpose. It doesn't matter how good you protect the login credentials: If someone already reads this, he's probably also able to debug
				// the crucial parts in Visual Studio, setting a breakpoint and read out all API keys. Therefore I created new API keys just for this program
				// So why the encryption at all? Because plaintext API keys on Github are searchable through a simple google search. Encrypted API keys are a bit harder to copy.
				var decryptor = Aes.Create();
				var key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
				var iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
				var decryptorTransformer = decryptor.CreateDecryptor(key, iv);

				string encryptedString = File.ReadAllText(@"resources\accounts.json");
				byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
				var decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
				var decryptedString = Encoding.UTF8.GetString(decryptedBytes);
				User.Accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedString);

				string plainString = File.ReadAllText(@"resources\settings.json");
				User.Settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(plainString);

				User.DbAccounts = new DataTable();
				User.DbAccounts.Locale = Runtime.CultEng;
				User.DbAccounts.Columns.Add("lastUsed", typeof(double));
				User.DbAccounts.Columns.Add("id", typeof(string));
				User.DbAccounts.Columns.Add("key", typeof(string));
				User.DbAccounts.Rows.Add(1, User.Accounts["DbAppId1"], User.Accounts["DbAppKey1"]);
				User.DbAccounts.Rows.Add(2, User.Accounts["DbAppId2"], User.Accounts["DbAppKey2"]);
				User.DbAccounts.Rows.Add(3, User.Accounts["DbAppId3"], User.Accounts["DbAppKey3"]);
				User.DbAccounts.Rows.Add(4, User.Accounts["DbAppId4"], User.Accounts["DbAppKey4"]);
				User.DbAccounts.Rows.Add(5, User.Accounts["DbAppId5"], User.Accounts["DbAppKey5"]);

				User.MgAccounts = new DataTable();
				User.MgAccounts.Locale = Runtime.CultEng;
				User.MgAccounts.Columns.Add("lastUsed", typeof(double));
				User.MgAccounts.Columns.Add("key", typeof(string));
				User.MgAccounts.Rows.Add(1, User.Accounts["MgAppKey1"]);
				User.MgAccounts.Rows.Add(2, User.Accounts["MgAppKey2"]);
				User.MgAccounts.Rows.Add(3, User.Accounts["MgAppKey3"]);

				User.MmAccounts = new DataTable();
				User.MmAccounts.Locale = Runtime.CultEng;
				User.MmAccounts.Columns.Add("lastUsed", typeof(double));
				User.MmAccounts.Columns.Add("key", typeof(string));
				User.MmAccounts.Rows.Add(1, User.Accounts["MmApiKey1"]);
				User.MmAccounts.Rows.Add(2, User.Accounts["MmApiKey2"]);
				User.MmAccounts.Rows.Add(3, User.Accounts["MmApiKey3"]);

				return true;
			}
			catch (NullReferenceException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}
			catch (JsonReaderException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}
			catch (KeyNotFoundException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}

			return false;
		}

		// ###########################################################################
		private string ConvertXmlToJson(string xmlstring)
		{
			XmlDocument xml = new XmlDocument();

			if (!string.IsNullOrWhiteSpace(xmlstring))
			{
				try
				{
					xml.LoadXml(xmlstring);
				}
				catch (XmlException error)
				{
					string[] errorMsg =
					{
						"ERROR: Could not convert XML to JSON",
						"XML string: " + xmlstring.TrimEnd('\r', '\n'),
						error.Message.Trim()
					};
					this.Log("error", errorMsg);
				}
			}

			string jsonstring = JsonConvert.SerializeXmlNode(xml);
			return jsonstring;
		}

		// ###########################################################################
		private JsonSerializerSettings GetJsonSettings()
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
			jsonSettings.Error += (obj, errorArgs) =>
			{
				this.Log("error", new[] { "ERROR: Could not convert server response to JSON", errorArgs.ErrorContext.Error.Message });

				errorArgs.ErrorContext.Handled = true;
			};
			return jsonSettings;
		}

		// ###########################################################################
		private bool IsValidMp3(string filepath)
		{
			byte[] fileHeader = new byte[3];
			byte[] mp3HeaderWithTags = { 0x49, 0x44, 0x33 };
			byte[] mp3HeaderWithoutTags = { 0xff, 0xfb };

			try
			{
				using (BinaryReader reader = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
				{
					reader.Read(fileHeader, 0, 3);
				}

				if (fileHeader.SequenceEqual(mp3HeaderWithTags))
				{
					return true;
				}

				fileHeader = fileHeader.Take(2).ToArray();
				if (fileHeader.SequenceEqual(mp3HeaderWithoutTags))
				{
					return true;
				}

				string message = string.Format(
									Runtime.CultEng,
									"ERROR: No valid MP3 file {0,93}",
									"file: \"" + filepath + "\"");
				this.Log("error", new[] { message });
				return false;
			}
			catch (IOException)
			{
				string message = string.Format(
									Runtime.CultEng,
									"ERROR: File in use by another program {0,93}",
									"file: \"" + filepath + "\"");
				this.Log("error", new[] { message });
				return false;
			}
		}

		// ###########################################################################
		private void MarkChange(int row, int col, string oldValue, string newValue)
		{
			if (!string.IsNullOrWhiteSpace(newValue) && oldValue != newValue)
			{
				try
				{
					this.dataGridView1[col, row].Value = newValue;

					if (string.IsNullOrWhiteSpace(oldValue))
					{
						this.dataGridView1[col, row].Style.BackColor = Color.LightGreen;
					}
					else
					{
						// TODO: use levenshtein distance to change file name if apropriate
						this.dataGridView1[col, row].Style.BackColor = Color.Yellow;
						this.dataGridView1[col, row].ToolTipText = oldValue;
					}
				}
				catch (ArgumentOutOfRangeException)
				{
					// User cleared all or current row. Therefore we would get a ArgumentOutOfRangeException. But we just continue
				}
			}
		}
		
		// ###########################################################################
		private void Log(string logtype, string[] values)
		{
			if (InvokeRequired)
			{
				Invoke(new Action<string, string[]>(this.Log), new object[] { values });
				return;
			}

			try
			{
				// Debug.WriteLine(message);
				string message = DateTime.Now.ToString("HH:mm:ss   ", Runtime.CultEng) + string.Join(Environment.NewLine + "           ", values);

				switch (logtype)
				{
					case "search":
						this.rtbSearchLog.AppendText(message + Environment.NewLine);
						this.rtbSearchLog.ScrollToCaret();
						break;
					case "error":
						this.rtbErrorLog.AppendText(message + Environment.NewLine);
						this.rtbErrorLog.ScrollToCaret();
						this.tabControl2.SelectTab(1);
						break;
					case "network":
						this.rtbNetworkLog.AppendText(message + Environment.NewLine);
						this.rtbNetworkLog.ScrollToCaret();
						break;
				}
			}
			catch (ObjectDisposedException)
			{
				// User closed window. Therefore richtextbox is alread disposed and not available for output. Nothing more to do here
			}
		}
	}
}
