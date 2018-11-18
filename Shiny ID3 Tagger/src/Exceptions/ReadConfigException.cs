//-----------------------------------------------------------------------
// <copyright file="ReadConfigException.cs" company="Shiny ID3 Tagger">
// Copyright (c) Shiny ID3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
//-----------------------------------------------------------------------

namespace Exceptions
{
	using System;

	/// <summary>
	/// Class that represents the custom UpdateClient exception
	/// </summary>
	internal class ReadConfigException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="ReadConfigException"/> class.
		/// </summary>
		public ReadConfigException()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadConfigException"/> class.
		/// </summary>
		/// <param name="message">The exeption message</param>
		public ReadConfigException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="ReadConfigException"/> class.
		/// </summary>
		/// <param name="message">The exeption message</param>
		/// <param name="innerException">The inner exception</param>
		public ReadConfigException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
