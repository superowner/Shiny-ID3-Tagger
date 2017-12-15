//-----------------------------------------------------------------------
// <copyright file="ValidateConfig.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Schema for settings.json</summary>
// Quick overview how schemas work:	https://spacetelescope.github.io/understanding-json-schema/basics.html
// Generate schema from existing JSON with real values: https://jsonschema.net/#/
// JSON.net schema documentation: https://www.newtonsoft.com/json/help/html/JTokenIsValidWithMessages.htm
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System.Collections.Generic;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Schema;

	public partial class Form1
	{
		#region schema for accounts.json
		private string accountsSchemaStr = @"
		{
		  ""definitions"": {},
		  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
		  ""$id"": ""http://example.com/example.json"",
		  ""type"": ""object"",
		  ""properties"": {
				""7dKey"": {
				  ""$id"": ""/properties/7dKey"",
				  ""type"": ""string""
				},
				""AmAssociateTag"": {
				  ""$id"": ""/properties/AmAssociateTag"",
				  ""type"": ""string""
				},
				""AmAccessKey"": {
				  ""$id"": ""/properties/AmAccessKey"",
				  ""type"": ""string""
				},
				""AmSecretKey"": {
				  ""$id"": ""/properties/AmSecretKey"",
				  ""type"": ""string""
				},
				""BaApiKey"": {
				  ""$id"": ""/properties/BaApiKey"",
				  ""type"": ""string""
				},
				""DbAppId1"": {
				  ""$id"": ""/properties/DbAppId1"",
				  ""type"": ""string""
				},
				""DbAppKey1"": {
				  ""$id"": ""/properties/DbAppKey1"",
				  ""type"": ""string""
				},
				""DbAppId2"": {
				  ""$id"": ""/properties/DbAppId2"",
				  ""type"": ""string""
				},
				""DbAppKey2"": {
				  ""$id"": ""/properties/DbAppKey2"",
				  ""type"": ""string""
				},
				""DbAppId3"": {
				  ""$id"": ""/properties/DbAppId3"",
				  ""type"": ""string""
				},
				""DbAppKey3"": {
				  ""$id"": ""/properties/DbAppKey3"",
				  ""type"": ""string""
				},
				""DbAppId4"": {
				  ""$id"": ""/properties/DbAppId4"",
				  ""type"": ""string""
				},
				""DbAppKey4"": {
				  ""$id"": ""/properties/DbAppKey4"",
				  ""type"": ""string""
				},
				""DbAppId5"": {
				  ""$id"": ""/properties/DbAppId5"",
				  ""type"": ""string""
				},
				""DbAppKey5"": {
				  ""$id"": ""/properties/DbAppKey5"",
				  ""type"": ""string""
				},
				""DcKey"": {
				  ""$id"": ""/properties/DcKey"",
				  ""type"": ""string""
				},
				""DcSecret"": {
				  ""$id"": ""/properties/DcSecret"",
				  ""type"": ""string""
				},
				""GeAccessToken"": {
				  ""$id"": ""/properties/GeAccessToken"",
				  ""type"": ""string""
				},
				""GnClientId"": {
				  ""$id"": ""/properties/GnClientId"",
				  ""type"": ""string""
				},
				""GnUserId"": {
				  ""$id"": ""/properties/GnUserId"",
				  ""type"": ""string""
				},
				""LaApiKey"": {
				  ""$id"": ""/properties/LaApiKey"",
				  ""type"": ""string""
				},
				""MgAppKey1"": {
				  ""$id"": ""/properties/MgAppKey1"",
				  ""type"": ""string""
				},
				""MgAppKey2"": {
				  ""$id"": ""/properties/MgAppKey2"",
				  ""type"": ""string""
				},
				""MgAppKey3"": {
				  ""$id"": ""/properties/MgAppKey3"",
				  ""type"": ""string""
				},
				""MmApiKey1"": {
				  ""$id"": ""/properties/MmApiKey1"",
				  ""type"": ""string""
				},
				""MmApiKey2"": {
				  ""$id"": ""/properties/MmApiKey2"",
				  ""type"": ""string""
				},
				""MmApiKey3"": {
				  ""$id"": ""/properties/MmApiKey3"",
				  ""type"": ""string""
				},
				""MsClientId"": {
				  ""$id"": ""/properties/MsClientId"",
				  ""type"": ""string""
				},
				""MsClientSecret"": {
				  ""$id"": ""/properties/MsClientSecret"",
				  ""type"": ""string""
				},
				""NaApiKey"": {
				  ""$id"": ""/properties/NaApiKey"",
				  ""type"": ""string""
				},
				""QoAppId"": {
				  ""$id"": ""/properties/QoAppId"",
				  ""type"": ""string""
				},
				""SpClientId"": {
				  ""$id"": ""/properties/SpClientId"",
				  ""type"": ""string""
				},
				""SpClientSecret"": {
				  ""$id"": ""/properties/SpClientSecret"",
				  ""type"": ""string""
				},
				""TiUsername"": {
				  ""$id"": ""/properties/TiUsername"",
				  ""type"": ""string""
				},
				""TiPassword"": {
				  ""$id"": ""/properties/TiPassword"",
				  ""type"": ""string""
				},
				""TiToken"": {
				  ""$id"": ""/properties/TiToken"",
				  ""type"": ""string""
				}
			  },
			  ""required"": [
				""7dKey"",
				""AmAssociateTag"",
				""AmAccessKey"",
				""AmSecretKey"",
				""BaApiKey"",
				""DbAppId1"",
				""DbAppKey1"",
				""DbAppId2"",
				""DbAppKey2"",
				""DbAppId3"",
				""DbAppKey3"",
				""DbAppId4"",
				""DbAppKey4"",
				""DbAppId5"",
				""DbAppKey5"",
				""DcKey"",
				""DcSecret"",
				""GeAccessToken"",
				""GnClientId"",
				""GnUserId"",
				""LaApiKey"",
				""MgAppKey1"",
				""MgAppKey2"",
				""MgAppKey3"",
				""MmApiKey1"",
				""MmApiKey2"",
				""MmApiKey3"",
				""MsClientId"",
				""MsClientSecret"",
				""NaApiKey"",
				""QoAppId"",
				""SpClientId"",
				""SpClientSecret"",
				""TiUsername"",
				""TiPassword"",
				""TiToken""
			  ]
		}";
		#endregion

		#region schema for settings.json
		private string settingsSchemaStr = @"
		{
		  ""definitions"": {},
		  ""$schema"": ""http://json-schema.org/draft-06/schema#"",
		  ""$id"": ""https://github.com/ShinyId3Tagger/Shiny-ID3-Tagger/settingsSchema.json"",
		  ""type"": ""object"",
		  ""properties"": {
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
				},
				""UserFrames"": {
				  ""$id"": ""/properties/UserFrames"",
				  ""type"": ""array"",
				  ""uniqueItems"": true,
				  ""items"": {
					""$id"": ""/properties/UserFrames/items"",
					""type"": ""string""
				  }
				},
				""DefaultFrames"": {
				  ""$id"": ""/properties/DefaultFrames"",
				  ""type"": ""array"",
				  ""uniqueItems"": true,
				  ""items"": {
					""$id"": ""/properties/DefaultFrames/items"",
					""type"": ""string""
				  }
				}
			  },
			  ""additionalProperties"": false,
			  ""required"": [
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
				""LyricsPriority"",
				""UserFrames"",
				""DefaultFrames""
			  ]
		}";
		#endregion

		private IList<string> ValidateConfig(string jsonStr, string schemaStr)
		{
			// Load schema
			JSchema schema = JSchema.Parse(schemaStr);

			// Load JSON
			JObject json = JObject.Parse(jsonStr);

			// Validate JSON against schema, save errors in errorMessages
			json.IsValid(schema, out IList<string> errorMessages);

			// Return all error messages why validation failed
			return errorMessages;
		}
	}
}
