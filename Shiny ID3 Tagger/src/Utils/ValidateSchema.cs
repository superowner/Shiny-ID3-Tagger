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
	using System.IO;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Schema;

	internal partial class Utils
	{
		internal static IList<string> ValidateSchema(string jsonStr, string schemaPath)
		{
			// Load schema
			using (StreamReader schemaFile = File.OpenText(schemaPath))
			using (JsonTextReader schemaReader = new JsonTextReader(schemaFile))
			{
				JSchema schema = JSchema.Load(schemaReader);

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
}
