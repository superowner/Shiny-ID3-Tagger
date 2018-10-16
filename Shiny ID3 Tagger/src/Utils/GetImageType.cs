//-----------------------------------------------------------------------
// <copyright file="GetImageType.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Checks if a given stream contains a valid image file signature</summary>
// https://stackoverflow.com/questions/1245567/finding-out-the-contenttype-of-a-image-from-the-byte/34677623#34677623
// https://www.garykessler.net/library/file_sigs.html
//-----------------------------------------------------------------------

namespace Utils
{
	using System.Collections.Generic;
	using System.Drawing.Imaging;
	using System.IO;
	using System.Linq;

	internal partial class Utils
	{
		private static Dictionary<byte[], ImageFormat> imageFormatDecoders = new Dictionary<byte[], ImageFormat>()
		{
			{ new byte[] { 0x42, 0x4D }, ImageFormat.Bmp },
			{ new byte[] { 0x47, 0x49, 0x46, 0x38, 0x37, 0x61 }, ImageFormat.Gif },
			{ new byte[] { 0x47, 0x49, 0x46, 0x38, 0x39, 0x61 }, ImageFormat.Gif },
			{ new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A }, ImageFormat.Png },
			{ new byte[] { 0xff, 0xd8 }, ImageFormat.Jpeg }
		};

		internal static bool IsValidImage(MemoryStream stream)
		{
			byte[] imageBytes = stream.ToArray();
			bool isImage = false;

			foreach (var imageType in imageFormatDecoders)
			{
				if (imageType.Key.SequenceEqual(imageBytes.Take(imageType.Key.Length)))
				{
					isImage = true;
				}
			}

			return isImage;
		}
	}
}
