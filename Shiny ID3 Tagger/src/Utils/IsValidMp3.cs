//-----------------------------------------------------------------------
// <copyright file="IsValidMp3.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

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
		/// Checks if a given filepath is a file and also a valid MP3 file
		/// Does this by inspecting first 3 bytes and comparing them with a known byte sequence
		/// </summary>
		/// <param name="filepath">File to check</param>
		/// <returns>True or false according to the previous check</returns>
		internal static bool IsValidMp3(string filepath)
		{
			// Prevents exception "ArgumentNullException"
			if (filepath == null)
			{
				return false;
			}

			byte[] fileHeader = new byte[3];
			byte[] mp3HeaderWithTags = { 0x49, 0x44, 0x33 }; // MP3 file where ID3 tag is present
			byte[] mp3HeaderWithoutTags = { 0xff, 0xfb };    // MP3 file where ID3 tag is not present

			// Catches possible exceptions
			// - ArgumentException
			// - ArgumentNullException
			// - ArgumentOutOfRangeException
			// - PathTooLongException
			// - UnauthorizedAccessException
			// - IOException
			// - DirectoryNotFoundException
			// - FileNotFoundException
			// - SystemException
			// - NotSupportedException
			try
			{
				// Try to read first 3 bytes of file at filepath
				using (BinaryReader binaryReader = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 100, true)))
				{
					binaryReader.Read(fileHeader, 0, 3);
				}
			}
			catch (Exception ex)
			{
				string[] errorMsg =
				{
					"ERROR:    File not found or inaccessible!",
					"File:     " + filepath,
					"Message:  " + ex.Message,
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
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
				string[] errorMsg =
				{
					"ERROR:    Not a valid MP3 file!",
					"file:     " + filepath,
				};
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
				return false;
			}
		}
	}
}
