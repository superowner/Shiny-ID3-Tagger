//-----------------------------------------------------------------------
// <copyright file="IsValidMp3.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Linq;
	using GlobalVariables;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Checks if a given file is a MP3 file
		/// Does this by inspecting first 3 bytes of a file and comparing them with known byte sequences for mp3 files
		/// </summary>
		/// <param name="filepath">File to check</param>
		/// <returns>True or false according to the previous check</returns>
		internal static bool IsValidMp3(string filepath)
		{
			byte[] fileHeader = new byte[3];
			byte[] mp3HeaderWithTags = { 0x49, 0x44, 0x33 }; // ID3 tags present
			byte[] mp3HeaderWithoutTags = { 0xff, 0xfb };    // ID3 tags not present but still a mp3 file

			try
			{
				// Read in first 3 bytes of file
				using (BinaryReader binaryReader = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 100, true)))
				{
					binaryReader.Read(fileHeader, 0, 3);
				}
			}
			catch (ArgumentException)
			{
				// If path points for example to a URL instead of a local file
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Invalid filepath!",
						"file:     " + filepath
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}

				return false;
			}
			catch (FileNotFoundException)
			{
				// If file is not found
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    File not found!",
						"file:     " + filepath
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}

				return false;
			}
			catch (IOException)
			{
				// If file has a write lock (i.e. opened in another program)
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Cannot access file. Already in use!",
						"file:     " + filepath
					};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}

				return false;
			}

			// Check for valid mp3 header bytes ( or )
			if (fileHeader.SequenceEqual(mp3HeaderWithTags) ||
				fileHeader.Take(2).ToArray().SequenceEqual(mp3HeaderWithoutTags))
			{
				return true;
			}
			else
			{
				// Print error message
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
							"ERROR:    Not a valid MP3 file!",
							"file:     " + filepath
						};
					Form1.Instance.RichTextBox_LogMessage(errorMsg);
				}

				// return false because MP3 file does not contain a valid mp3 header
				return false;
			}
		}
	}
}
