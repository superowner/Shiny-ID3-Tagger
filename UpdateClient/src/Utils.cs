//-----------------------------------------------------------------------
// <copyright file="Utils.cs" company="Shiny ID3 Tagger">
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
	internal class Utils
	{
		/// <summary>
		/// Blacklist for CopyAll(). These files wont get uddated. Needed to retain user made changes
		/// </summary>
		private static readonly string[] BlackList =
		{
			@"\config\accounts.user.json",
			@"\config\settings.user.json",
		};

		/// <summary>
		/// Copies one folder and its files to another - including all sub directories
		/// </summary>
		/// <param name="source">Full path of source folder</param>
		/// <param name="target">Full path of target folder</param>
		public static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
		{
			Directory.CreateDirectory(target.FullName);

			// Copy each file into the new directory
			foreach (FileInfo fi in source.GetFiles())
			{
				Console.WriteLine(@"Copying {0}\{1}", target.FullName, fi.Name);
				string targetFullPath = Path.Combine(target.FullName, fi.Name);
				fi.CopyTo(targetFullPath, true);
			}

			// Copy each subdirectory using recursion
			foreach (DirectoryInfo diSourceSubDir in source.GetDirectories())
			{
				DirectoryInfo nextTargetSubDir = target.CreateSubdirectory(diSourceSubDir.Name);

				CopyDirectory(diSourceSubDir, nextTargetSubDir);
			}
		}
	}
}
