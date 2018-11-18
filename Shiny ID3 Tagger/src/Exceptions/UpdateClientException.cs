//-----------------------------------------------------------------------
// <copyright file="UpdateClientException.cs" company="Shiny ID3 Tagger">
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
	internal class UpdateClientException : Exception
	{
		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateClientException"/> class.
		/// </summary>
		public UpdateClientException()
			: base()
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateClientException"/> class.
		/// </summary>
		/// <param name="message">The exeption message</param>
		public UpdateClientException(string message)
			: base(message)
		{
		}

		/// <summary>
		/// Initializes a new instance of the <see cref="UpdateClientException"/> class.
		/// </summary>
		/// <param name="message">The exeption message</param>
		/// <param name="innerException">The inner exception</param>
		public UpdateClientException(string message, Exception innerException)
			: base(message, innerException)
		{
		}
	}
}
