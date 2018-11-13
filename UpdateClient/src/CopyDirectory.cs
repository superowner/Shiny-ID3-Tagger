//-----------------------------------------------------------------------
// <copyright file="CopyDirectory.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace UpdateClient
{
	using System;
	using System.IO;

	/// <summary>
	/// Represents the Utils class for helper methods
	/// </summary>
	internal class CopyDirectory
	{
		/// <summary>
		/// Copies one folder and its files to another - including all sub directories
		/// </summary>
		/// <param name="source">Full path of source folder</param>
		/// <param name="target">Full path of target folder</param>
		public static void CopyAll(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);

			// Copy each file into the new directory.
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				fi.CopyTo(Path.Combine(target.FullName, fi.Name), true);
			}

			// Copy each subdirectory using recursion.
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir =
					target.CreateSubdirectory(diSourceSubDir.Name);
				CopyAll(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}
