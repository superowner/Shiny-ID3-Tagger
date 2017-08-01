//-----------------------------------------------------------------------
// <copyright file="ReadAccountCredentials.cs" company="Shiny Id3 Tagger">
//	 Copyright (c) Shiny Id3 Tagger. All rights reserved.
// </copyright>
// <author>ShinyId3Tagger Team</author>
// <summary>Reads and decrypts all values from settings.json and account.json. Executed at program start</summary>
//-----------------------------------------------------------------------

namespace GlobalNamespace
{
	using System;
	using System.Collections.Generic;
	using System.Data;
	using System.IO;
	using System.Linq;
	using System.Security.Cryptography;
	using System.Text;
	using Newtonsoft.Json;

	public partial class Form1
	{
		private bool ReadAccountCredentials()
		{
			try
			{
				// The key for decryption is hardcoded on purpose. It doesn't matter how good you protect the login credentials: If someone already reads this, he's probably also able to debug
				// the crucial parts in Visual Studio, setting a breakpoint and read out all API keys. Therefore I created new API keys just for this program
				// So why the encryption at all? Because plaintext API keys on Github are searchable through a simple google search. Encrypted API keys are a bit harder to copy.
				var decryptor = Aes.Create();
				var key = new byte[] { 90, 181, 178, 196, 110, 221, 12, 79, 98, 48, 40, 239, 237, 175, 23, 69, 42, 201, 36, 157, 170, 67, 161, 9, 69, 114, 232, 179, 195, 158, 151, 124 };
				var iv = new byte[] { 221, 237, 248, 138, 53, 16, 87, 148, 28, 20, 30, 199, 195, 221, 209, 188 };
				var decryptorTransformer = decryptor.CreateDecryptor(key, iv);
				
				string encryptedString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\resources\accounts.json");
				byte[] encryptedBytes = Convert.FromBase64String(encryptedString);
				var decryptedBytes = decryptorTransformer.TransformFinalBlock(encryptedBytes, 0, encryptedBytes.Length);
				var decryptedString = Encoding.UTF8.GetString(decryptedBytes);
				User.Accounts = JsonConvert.DeserializeObject<Dictionary<string, string>>(decryptedString);

				string plainString = File.ReadAllText(AppDomain.CurrentDomain.BaseDirectory + @"\resources\settings.json");
				User.Settings = JsonConvert.DeserializeObject<Dictionary<string, dynamic>>(plainString);

				User.DbAccounts = new DataTable();
				User.DbAccounts.Columns.Add("lastUsed", typeof(double));
				User.DbAccounts.Columns.Add("id", typeof(string));
				User.DbAccounts.Columns.Add("key", typeof(string));
				User.DbAccounts.Rows.Add(1, User.Accounts["DbAppId1"], User.Accounts["DbAppKey1"]);
				User.DbAccounts.Rows.Add(2, User.Accounts["DbAppId2"], User.Accounts["DbAppKey2"]);
				User.DbAccounts.Rows.Add(3, User.Accounts["DbAppId3"], User.Accounts["DbAppKey3"]);
				User.DbAccounts.Rows.Add(4, User.Accounts["DbAppId4"], User.Accounts["DbAppKey4"]);
				User.DbAccounts.Rows.Add(5, User.Accounts["DbAppId5"], User.Accounts["DbAppKey5"]);

				User.MgAccounts = new DataTable();
				User.MgAccounts.Columns.Add("lastUsed", typeof(double));
				User.MgAccounts.Columns.Add("key", typeof(string));
				User.MgAccounts.Rows.Add(1, User.Accounts["MgAppKey1"]);
				User.MgAccounts.Rows.Add(2, User.Accounts["MgAppKey2"]);
				User.MgAccounts.Rows.Add(3, User.Accounts["MgAppKey3"]);

				// TODO Set locale
				User.MmAccounts = new DataTable();
				User.MmAccounts.Columns.Add("lastUsed", typeof(double));
				User.MmAccounts.Columns.Add("key", typeof(string));
				User.MmAccounts.Rows.Add(1, User.Accounts["MmApiKey1"]);
				User.MmAccounts.Rows.Add(2, User.Accounts["MmApiKey2"]);
				User.MmAccounts.Rows.Add(3, User.Accounts["MmApiKey3"]);

				return true;
			}
			catch (NullReferenceException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}
			catch (JsonReaderException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}
			catch (KeyNotFoundException)
			{
				this.Log("error", new[] { @"ERROR: Could not read all values from 'resources\settings.json' or 'resources\accounts.json'" });
			}

			return false;
		}
	}
}
