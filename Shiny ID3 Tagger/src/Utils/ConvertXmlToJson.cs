//-----------------------------------------------------------------------
// <copyright file="ConvertXmlToJson.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Xml;
	using GlobalVariables;
	using Newtonsoft.Json;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Converts a string from XML notation to JSON notation. Throws detailed errors if conversion fails
		/// </summary>
		/// <param name="xmlStr">The input XML string</param>
		/// <returns>The JSON string after converting from XML notation</returns>
		internal static string ConvertXmlToJson(string xmlStr)
		{
			XmlDocument xml = new XmlDocument();

			if (!string.IsNullOrWhiteSpace(xmlStr))
			{
				try
				{
					xml.LoadXml(xmlStr);
				}
				catch (XmlException error)
				{
					string[] warningMsg =
					{
						"WARNING:  Could not convert XML to JSON!",
						"String:   " + xmlStr.TrimEnd('\r', '\n'),
						"Message:  " + error.Message.Trim(),
					};
					Form1.Instance.RichTextBox_LogMessage(warningMsg, 3);
				}
			}

			string jsonStr = JsonConvert.SerializeXmlNode(xml);
			return jsonStr;
		}
	}
}
