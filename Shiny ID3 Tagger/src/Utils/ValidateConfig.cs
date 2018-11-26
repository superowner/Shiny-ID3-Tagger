//-----------------------------------------------------------------------
// <copyright file="ValidateConfig.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

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
		/// Validates a given JSON against a predefined schema and returns a list of errors of it fails
		/// Quick overview on how schemas work:	<seealso href="https://spacetelescope.github.io/understanding-json-schema/basics.html"/>
		/// Generate schema from existing JSON with real values: <seealso href="https://jsonschema.net/#/"/>
		/// JSON schema validator: <seealso href="https://www.jsonschemavalidator.net/"/>
		/// JSON.net schema documentation: <seealso href="https://www.newtonsoft.com/json/help/html/JTokenIsValidWithMessages.htm"/>
		/// </summary>
		/// <param name="json">The JSON to validate</param>
		/// <param name="schemaPath">The path to the schema file</param>
		/// <returns>A list of error messages as strings</returns>
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
			catch (Exception ex)
			{
				return new List<string> { ex.Message };
			}
		}
	}
}
