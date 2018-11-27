//-----------------------------------------------------------------------
// <copyright file="DeleteFileOrFolder.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------
// Reviewed and checked if all possible exceptions are prevented or handled

namespace Utils
{
	using System;
	using System.IO;
	using System.Threading.Tasks;
	using Shiny_ID3_Tagger;

	/// <summary>
	/// Represents the Utility class which holds various helper functions
	/// </summary>
	internal partial class Utils
	{
		/// <summary>
		/// Deletes a given file or folder
		/// Retries three times with a short delay if deletion failed
		/// </summary>
		/// <param name="fullPath">The path to a file/folder</param>
		/// <returns>True if deletion was successfull or false if not</returns>
		internal static async Task<bool> DeleteFileOrFolder(string fullPath)
		{
			const int WriteDelay = 50;
			const int MaxRetries = 3;
			bool isDeleted = false;
			string[] errorMsg = null;

			if (Directory.Exists(fullPath) || File.Exists(fullPath))
			{
				for (int retry = 0; retry < MaxRetries; retry++)
				{
					if (File.Exists(fullPath))
					{
						try
						{
							File.Delete(fullPath);
						}
						catch (Exception ex)
						{
							errorMsg = new string[]
							{
								"ERROR:    Could not delete file!",
								"File:     " + fullPath,
								"Message:  " + ex.Message,
							};
							await Task.Delay(WriteDelay);
						}
					}

					if (Directory.Exists(fullPath))
					{
						try
						{
							Directory.Delete(fullPath, true);
						}
						catch (Exception ex)
						{
							errorMsg = new string[]
							{
								"ERROR:    Could not delete folder!",
								"File:     " + fullPath,
								"Message:  " + ex.Message,
							};
							await Task.Delay(WriteDelay);
						}
					}

					if (Directory.Exists(fullPath) == false && File.Exists(fullPath) == false)
					{
						isDeleted = true;
						break;
					}
				}
			}
			else
			{
				isDeleted = true;
			}

			if (isDeleted == false)
			{
				Form1.Instance.RichTextBox_LogMessage(errorMsg, 2);
			}

			return isDeleted;
		}
	}
}
