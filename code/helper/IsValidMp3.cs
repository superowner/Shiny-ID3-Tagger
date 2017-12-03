//-----------------------------------------------------------------------
// <copyright file="IsValidMp3.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given file is a real MP3 by inspecting the first 3 bytes in file header</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.IO;
	using System.Linq;
	
	public partial class Form1
	{
		private bool IsValidMp3(string filepath)
		{
			byte[] fileHeader = new byte[3];
			byte[] mp3HeaderWithTags = { 0x49, 0x44, 0x33 };
			byte[] mp3HeaderWithoutTags = { 0xff, 0xfb };

			try
			{
				using (BinaryReader reader = new BinaryReader(new FileStream(filepath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite)))
				{
					reader.Read(fileHeader, 0, 3);
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
				if (User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Not a valid MP3 file!",
						"file:     " + filepath
					};
					this.PrintLogMessage("error", errorMsg);
				}
				
				return false;
			}
			catch (FileNotFoundException)
			{
				// If file is not found
				if (User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    File not found!",
						"file:     " + filepath
					};
					this.PrintLogMessage("error", errorMsg);
				}

				return false;				
			}
			catch (IOException)
			{
				// If file has a write lock (ie. opened in another program)
				if (User.Settings["DebugLevel"] >= 1)
				{
					string[] errorMsg =
					{
						"ERROR:    Cannot access file. Already in use!",
						"file:     " + filepath
					};
					this.PrintLogMessage("error", errorMsg);
				}
								
				return false;
			}
		}
	}
}
