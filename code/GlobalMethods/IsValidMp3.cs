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
	using System.Diagnostics;
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

				if (fileHeader.SequenceEqual(mp3HeaderWithTags))
				{
					return true;
				}

				fileHeader = fileHeader.Take(2).ToArray();
				if (fileHeader.SequenceEqual(mp3HeaderWithoutTags))
				{
					return true;
				}

				string message = string.Format(
									Runtime.CultEng,
									"{0,-100}{1}",
									"ERROR: Not a valid MP3 file",
									"file: \"" + filepath + "\"");
				this.Log("error", new[] { message });
				return false;
			}
			catch (FileNotFoundException)
			{
				string message = string.Format(
									Runtime.CultEng,
									"{0,-100}{1}",
									"ERROR: File not found",
									"file: \"" + filepath + "\"");
				this.Log("error", new[] { message });
				return false;				
			}
			catch (IOException)
			{
				string message = string.Format(
									Runtime.CultEng,
									"{0,-100}{1}",
									"ERROR: Cannot access file. Already in use",
									"file: \"" + filepath + "\"");
				this.Log("error", new[] { message });
				return false;
			}
		}
	}
}
