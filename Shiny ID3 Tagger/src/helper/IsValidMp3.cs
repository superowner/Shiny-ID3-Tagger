//-----------------------------------------------------------------------
// <copyright file="IsValidMp3.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given file is a real MP3 by inspecting first 3 bytes in file header</summary>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Linq;
    using GlobalNamespace;
    using GlobalVariables;

	internal partial class Utils
	{
		internal static bool IsValidMp3(string filepath)
		{
			byte[] fileHeader = new byte[3];
			byte[] mp3HeaderWithTags = { 0x49, 0x44, 0x33 };
			byte[] mp3HeaderWithoutTags = { 0xff, 0xfb };

			try
			{
				// Read in first 3 bytes of file
				using (BinaryReader binaryReader = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite, 100, true)))
				{
					binaryReader.Read(fileHeader, 0, 3);
				}

				// Check for valid mp3 header bytes (ID3 tags present)
				if (fileHeader.SequenceEqual(mp3HeaderWithTags))
				{
					return true;
				}

				// Check for valid mp3 header bytes (no ID3 tags present)
				fileHeader = fileHeader.Take(2).ToArray();
				if (fileHeader.SequenceEqual(mp3HeaderWithoutTags))
				{
					return true;
				}

				// If mp3 file does not contain valid mp3 header bytes, print error message
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Not a valid MP3 file!",
						"file:     " + filepath
					};
					Form1.Instance.PrintErrorMessage(errorMsg);
				}

				return false;
			}
			catch (ArgumentException)
			{
				// If path points to a URL
				if ((int)User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Invalid filepath!",
						"file:     " + filepath
					};
					Form1.Instance.PrintErrorMessage(errorMsg);
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
					Form1.Instance.PrintErrorMessage(errorMsg);
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
					Form1.Instance.PrintErrorMessage(errorMsg);
				}

				return false;
			}
		}
	}
}
