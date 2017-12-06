//-----------------------------------------------------------------------
// <copyright file="GetJsonSettings.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Sets non-default settings for all JSON deserializations. This is helpful for debugging invalid JSON strings</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using Newtonsoft.Json;

	public partial class Form1
	{
		private JsonSerializerSettings GetJsonSettings()
		{
			JsonSerializerSettings jsonSettings = new JsonSerializerSettings();
			jsonSettings.Error += (obj, errorArgs) =>
			{
				if (User.Settings["DebugLevel"] >= 2)
				{
					string[] errorMsg =
					{
						"WARNING:  Could not convert response to JSON!",
						"Message:  " + errorArgs.ErrorContext.Error.Message.TrimEnd('\r', '\n')
					};
					this.PrintLogMessage("error", errorMsg);
				}
			};

			jsonSettings.Formatting = Formatting.Indented;

			return jsonSettings;
		}
	}
}
