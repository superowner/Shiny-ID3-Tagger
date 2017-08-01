//-----------------------------------------------------------------------
// <copyright file="GetJsonSettings.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sets non-default settings for all JSON deserializations. This is helpful for debugging invalid JSON strings</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Linq;
	using Newtonsoft.Json;

	public partial class Form1
	{
		private JsonSerializerSettings GetJsonSettings()
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
			jsonSettings.Error += (obj, errorArgs) =>
			{
				this.Log("error", new[] { "ERROR: Could not convert server response to JSON", errorArgs.ErrorContext.Error.Message });
				errorArgs.ErrorContext.Handled = true;
			};
			
			jsonSettings.Formatting = Formatting.Indented;
			
			return jsonSettings;
		}
	}
}
