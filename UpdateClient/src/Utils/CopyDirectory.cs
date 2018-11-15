//-----------------------------------------------------------------------
// <copyright file="CopyDirectory.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Utils
{
	using System;
	using System.IO;
	using System.Linq;

	/// <summary>
	/// Represents the Utils class for helper methods
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Blacklist for CopyDirectory() method. These files wont get copied/uddated so user changes are retained
		/// </summary>
		private static readonly string[] BlackListedFiles =
		{
			AppDomain.CurrentDomain.BaseDirectory + @"config\accounts.user.json",
			AppDomain.CurrentDomain.BaseDirectory + @"config\settings.user.json",
			AppDomain.CurrentDomain.BaseDirectory + @"update.log",
		};

		/// <summary>
		/// Copies one folder and its files to another - including all sub directories
		/// </summary>
		/// <param name="source">Full path of source folder</param>
		/// <param name="target">Full path of target folder</param>
		internal static void CopyDirectory(DirectoryInfo source, DirectoryInfo target)
		{
			Utils.ConsoleWriteLine(string.Format(@"Copying {0}\", target.FullName));
			Directory.CreateDirectory(target.FullName);

			// Exclude blacklisted files
			FileInfo[] filteredFiles = (from file in source.GetFiles()
										where BlackListedFiles.Contains(file.FullName) == false
										select file).ToArray();

			// Copy each file into the new directory
			foreach (FileInfo fi in filteredFiles)
			{
				Utils.ConsoleWriteLine(string.Format(@"Copying {0}\{1}", target.FullName, fi.Name));
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
