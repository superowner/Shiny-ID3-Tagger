//-----------------------------------------------------------------------
// <copyright file="ValidateConfig.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Review: Use $ref and $id to get rid of settings.user.schema. Use settings.default.json as schema then
// 			The problem is, how can i have one schema with "required" and one without "required"
// 			https://www.newtonsoft.com/jsonschema/help/html/LoadingSchemas.htm

namespace Utils
{
	using System;
	using System.Collections.Generic;
	using System.IO;
	using Exceptions;
	using Newtonsoft.Json;
	using Newtonsoft.Json.Linq;
	using Newtonsoft.Json.Schema;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Validates a given JSON against a predefined schema and throws an error of it fails
		/// Quick overview on how schemas work:	<seealso href="https://spacetelescope.github.io/understanding-json-schema/basics.html"/>
		/// Generate schema from existing JSON with real values: <seealso href="https://jsonschema.net/#/"/>
		/// JSON.net schema documentation:<seealso href="https://www.newtonsoft.com/json/help/html/JTokenIsValidWithMessages.htm"/>
		/// </summary>
		/// <param name="json">The JSON to validate</param>
		/// <param name="schemaPath">The path to the schema file</param>
		/// <returns>A tuple with a bool and a list of strings</returns>
		internal static List<string> ValidateConfig(JObject json, string schemaPath)
		{
			try
			{
				using (StreamReader schemaFile = File.OpenText(schemaPath))
				using (JsonTextReader schemaReader = new JsonTextReader(schemaFile))
				{
					// Read schema
					JSchema schema = JSchema.Load(schemaReader);

					// Validate JSON against schema, save errors in errorMessages list
					json.IsValid(schema, out IList<string> errorMessages);

					return (List<string>)errorMessages;
				}
			}
			catch (Exception)
			{
				return new List<string> { "hey" };
			}
		}
	}
}
