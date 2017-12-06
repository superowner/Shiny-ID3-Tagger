//-----------------------------------------------------------------------
// <copyright file="ConvertXmlToJson.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Converts a string from XML notation to JSON notation</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System.Xml;
	using Newtonsoft.Json;

	public partial class Form1
	{
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
					if (User.Settings["DebugLevel"] >= 2)
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert XML to JSON!",
							"String:   " + xmlstring.TrimEnd('\r', '\n'),
							"Message:  " + error.Message.Trim()
						};
						this.PrintLogMessage("error", errorMsg);
					}
				}
			}

			string jsonstring = JsonConvert.SerializeXmlNode(xml);
			return jsonstring;
		}
	}
}
