//-----------------------------------------------------------------------
// <copyright file="ConvertXmlToJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Converts a string from XML notation to JSON notation</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Xml;
	using GlobalNamespace;
	using GlobalVariables;
	using Newtonsoft.Json;

	internal partial class Utils
	{
		internal static string ConvertXmlToJson(string xmlString)
		{
			XmlDocument xml = new XmlDocument();

			if (!string.IsNullOrWhiteSpace(xmlString))
			{
				try
				{
					xml.LoadXml(xmlString);
				}
				catch (XmlException error)
				{
					if ((int)User.Settings["DebugLevel"] >= 2)
					{
						string[] errorMsg =
						{
							"WARNING:  Could not convert XML to JSON!",
							"String:   " + xmlString.TrimEnd('\r', '\n'),
							"Message:  " + error.Message.Trim()
						};
						Form1.Instance.PrintErrorMessage(errorMsg);
					}
				}
			}

			string jsonStr = JsonConvert.SerializeXmlNode(xml);
			return jsonStr;
		}
	}
}
