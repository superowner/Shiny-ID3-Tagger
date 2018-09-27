//-----------------------------------------------------------------------
// <copyright file="ValidateSchema.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Schema validator for various JSON files</summary>
// Quick overview how schemas work:	https://spacetelescope.github.io/understanding-json-schema/basics.html
// Generate schema from existing JSON with real values: https://jsonschema.net/#/
// JSON.net schema documentation: https://www.newtonsoft.com/json/help/html/JTokenIsValidWithMessages.htm
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Schema;

	public partial class Utils
	{
		#region schema for accounts.json
		private static string accountsSchemaStr = @"
		{
		  ""definitions"": {},
		  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
		  ""$id"": ""http://example.com/example.json"",
		  ""type"": ""object"",
		  ""properties"": {
			  ""GitHub"": {
			  ""$id"": ""/properties/GitHub"",
			  ""type"": ""object"",
			  ""properties"": {
				""AccessToken"": {
				  ""$id"": ""/properties/GitHub/properties/AccessToken"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""AccessToken""
			  ]
			},
			""7digital"": {
			  ""$id"": ""/properties/7digital"",
			  ""type"": ""object"",
			  ""properties"": {
				""key"": {
				  ""$id"": ""/properties/7digital/properties/key"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""key""
			  ]
			},
			""Amazon"": {
			  ""$id"": ""/properties/Amazon"",
			  ""type"": ""object"",
			  ""properties"": {
				""AssociateTag"": {
				  ""$id"": ""/properties/Amazon/properties/AssociateTag"",
				  ""type"": ""string""
				},
				""AccessKey"": {
				  ""$id"": ""/properties/Amazon/properties/AccessKey"",
				  ""type"": ""string""
				},
				""SecretKey"": {
				  ""$id"": ""/properties/Amazon/properties/SecretKey"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""AssociateTag"",
				""AccessKey"",
				""SecretKey""
			  ]
			},
			""Decibel"": {
			  ""$id"": ""/properties/Decibel"",
			  ""type"": ""array"",
			  ""uniqueItems"": true,
			  ""items"": {
				""$id"": ""/properties/Decibel/items"",
				""type"": ""object"",
				""properties"": {
				  ""AppId"": {
					""$id"": ""/properties/Decibel/items/properties/AppId"",
					""type"": ""string""
				  },
				  ""AppKey"": {
					""$id"": ""/properties/Decibel/items/properties/AppKey"",
					""type"": ""string""
				  }
				},
				""required"": [
				  ""AppId"",
				  ""AppKey""
				]
			  }
			},
			""Discogs"": {
			  ""$id"": ""/properties/Discogs"",
			  ""type"": ""object"",
			  ""properties"": {
				""Key"": {
				  ""$id"": ""/properties/Discogs/properties/Key"",
				  ""type"": ""string""
				},
				""Secret"": {
				  ""$id"": ""/properties/Discogs/properties/Secret"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""Key"",
				""Secret""
			  ]
			},
			""Genius"": {
			  ""$id"": ""/properties/Genius"",
			  ""type"": ""object"",
			  ""properties"": {
				""AccessToken"": {
				  ""$id"": ""/properties/Genius/properties/AccessToken"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""AccessToken""
			  ]
			},
			""Gracenote"": {
			  ""$id"": ""/properties/Gracenote"",
			  ""type"": ""object"",
			  ""properties"": {
				""ClientId"": {
				  ""$id"": ""/properties/Gracenote/properties/ClientId"",
				  ""type"": ""string""
				},
				""UserId"": {
				  ""$id"": ""/properties/Gracenote/properties/UserId"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""ClientId"",
				""UserId""
			  ]
			},
			""Lastfm"": {
			  ""$id"": ""/properties/Lastfm"",
			  ""type"": ""object"",
			  ""properties"": {
				""ApiKey"": {
				  ""$id"": ""/properties/Lastfm/properties/ApiKey"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""ApiKey""
			  ]
			},
			""MusicGraph"": {
			  ""$id"": ""/properties/MusicGraph"",
			  ""type"": ""array"",
			  ""uniqueItems"": true,
			  ""items"": {
				""$id"": ""/properties/MusicGraph/items"",
				""type"": ""object"",
				""properties"": {
				  ""AppKey"": {
					""$id"": ""/properties/MusicGraph/items/properties/AppKey"",
					""type"": ""string""
				  }
				},
				""required"": [
				  ""AppKey""
				]
			  }
			},
			""Musixmatch"": {
			  ""$id"": ""/properties/Musixmatch"",
			  ""type"": ""array"",
			  ""uniqueItems"": true,
			  ""items"": {
				""$id"": ""/properties/Musixmatch/items"",
				""type"": ""object"",
				""properties"": {
				  ""ApiKey"": {
					""$id"": ""/properties/Musixmatch/items/properties/ApiKey"",
					""type"": ""string""
				  }
				},
				""required"": [
				  ""ApiKey""
				]
			  }
			},
			""MsGroove"": {
			  ""$id"": ""/properties/MsGroove"",
			  ""type"": ""object"",
			  ""properties"": {
				""ClientId"": {
				  ""$id"": ""/properties/MsGroove/properties/ClientId"",
				  ""type"": ""string""
				},
				""ClientSecret"": {
				  ""$id"": ""/properties/MsGroove/properties/ClientSecret"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""ClientId"",
				""ClientSecret""
			  ]
			},
			""Napster"": {
			  ""$id"": ""/properties/Napster"",
			  ""type"": ""object"",
			  ""properties"": {
				""ApiKey"": {
				  ""$id"": ""/properties/Napster/properties/ApiKey"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""ApiKey""
			  ]
			},
			""Qobuz"": {
			  ""$id"": ""/properties/Qobuz"",
			  ""type"": ""object"",
			  ""properties"": {
				""AppId"": {
				  ""$id"": ""/properties/Qobuz/properties/AppId"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""AppId""
			  ]
			},
			""Spotify"": {
			  ""$id"": ""/properties/Spotify"",
			  ""type"": ""object"",
			  ""properties"": {
				""ClientId"": {
				  ""$id"": ""/properties/Spotify/properties/ClientId"",
				  ""type"": ""string""
				},
				""ClientSecret"": {
				  ""$id"": ""/properties/Spotify/properties/ClientSecret"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""ClientId"",
				""ClientSecret""
			  ]
			},
			""Tidal"": {
			  ""$id"": ""/properties/Tidal"",
			  ""type"": ""object"",
			  ""properties"": {
				""Username"": {
				  ""$id"": ""/properties/Tidal/properties/Username"",
				  ""type"": ""string""
				},
				""Password"": {
				  ""$id"": ""/properties/Tidal/properties/Password"",
				  ""type"": ""string""
				},
				""Token"": {
				  ""$id"": ""/properties/Tidal/properties/Token"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""Username"",
				""Password"",
				""Token""
			  ]
			}
		  },
		  ""required"": [
			""7digital"",
			""Amazon"",
			""Decibel"",
			""Discogs"",
			""Genius"",
			""Gracenote"",
			""Lastfm"",
			""MusicGraph"",
			""Musixmatch"",
			""MsGroove"",
			""Napster"",
			""Qobuz"",
			""Spotify"",
			""Tidal""
		  ]
		}";
		#endregion

		#region schema for settings.json
		private static string settingsSchemaStr = @"
		{
		  ""definitions"": {},
		  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
		  ""$id"": ""https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/settingsSchema.json"",
		  ""type"": ""object"",
		  ""properties"": {
				""Branch"": {
				  ""$id"": ""/properties/Branch"",
				  ""type"": ""string"",
				  ""enum"": [""master"", ""develop""]
				},
				""DebugLevel"": {
				  ""$id"": ""/properties/DebugLevel"",
				  ""type"": ""integer""
				},
				""PreferTags"": {
				  ""$id"": ""/properties/PreferTags"",
				  ""type"": ""boolean""
				},
				""RemoveBrackets"": {
				  ""$id"": ""/properties/RemoveBrackets"",
				  ""type"": ""boolean""
				},
				""RemoveFeaturing"": {
				  ""$id"": ""/properties/RemoveFeaturing"",
				  ""type"": ""boolean""
				},
				""OverwriteImage"": {
				  ""$id"": ""/properties/OverwriteImage"",
				  ""type"": ""boolean""
				},
				""AutoSearch"": {
				  ""$id"": ""/properties/AutoSearch"",
				  ""type"": ""boolean""
				},
				""AutoCapitalize"": {
				  ""$id"": ""/properties/AutoCapitalize"",
				  ""type"": ""boolean""
				},
				""ThresholdRedValue"": {
				  ""$id"": ""/properties/ThresholdRedValue"",
				  ""type"": ""integer""
				},
				""MaxImageSize"": {
				  ""$id"": ""/properties/MaxImageSize"",
				  ""type"": ""integer""
				},
				""UserAgent"": {
				  ""$id"": ""/properties/UserAgent"",
				  ""type"": ""string""
				},
				""FilenamePatterns"": {
				  ""$id"": ""/properties/FilenamePatterns"",
				  ""type"": ""array"",
				  ""uniqueItems"": true,
				  ""items"": {
					""$id"": ""/properties/FilenamePatterns/items"",
					""type"": ""string""
				  }
				},
				""CoverPriority"": {
				  ""$id"": ""/properties/CoverPriority"",
				  ""type"": ""array"",
				  ""uniqueItems"": true,
				  ""items"": {
					""$id"": ""/properties/CoverPriority/items"",
					""type"": ""string""
				  }
				},
				""LyricsPriority"": {
				  ""$id"": ""/properties/LyricsPriority"",
				  ""type"": ""array"",
				  ""uniqueItems"": true,
				  ""items"": {
					""$id"": ""/properties/LyricsPriority/items"",
					""type"": ""string""
				  }
				}
			  },
			  ""additionalProperties"": false,
			  ""required"": [
				""Branch"",
				""DebugLevel"",
				""PreferTags"",
				""RemoveBrackets"",
				""RemoveFeaturing"",
				""OverwriteImage"",
				""AutoSearch"",
				""AutoCapitalize"",
				""ThresholdRedValue"",
				""MaxImageSize"",
				""UserAgent"",
				""FilenamePatterns"",
				""CoverPriority"",
				""LyricsPriority""
			  ]
		}";
		#endregion

		#region schema for lastCommit.json
		private static string lastCommitSchemaStr = @"
		{
		  ""definitions"": {},
		  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
		  ""$id"": ""https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/lastCommitSchema.json"",
		  ""type"": ""object"",
		  ""properties"": {
				""commit"": {
				  ""$id"": ""/properties/commit"",
				  ""type"": ""string"",
				},
				""date"": {
				  ""$id"": ""/properties/date"",
				  ""type"": ""string""
				}
			  },
			  ""additionalProperties"": false,
			  ""required"": [
				""commit"",
				""date""
			  ]
		}";
		#endregion

		public static IList<string> ValidateSchema(string jsonStr, string schemaStr)
		{
			// Load schema
			JSchema schema = JSchema.Parse(schemaStr);

			// Load JSON
			JObject json = JObject.Parse(jsonStr);

			// Validate JSON against schema, save errors in errorMessages
			json.IsValid(schema, out IList<string> errorMessages);

			// If any validation error occurred, throw exception to go into catch clause
			if (errorMessages.Count > 0)
			{
				string allValidationErrors = string.Join("\n          ", (IEnumerable<string>)errorMessages);

				throw new ArgumentException(allValidationErrors);
			}

			// Return all error messages why validation failed
			return errorMessages;
		}
	}
}
