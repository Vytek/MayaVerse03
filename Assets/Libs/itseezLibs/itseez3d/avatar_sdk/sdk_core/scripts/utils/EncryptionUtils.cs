/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System;
using System.Security.Cryptography;
using System.Text;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Utilities for symmetric encryption/decryption.
	/// </summary>
	public class EncryptionUtils
	{
		/// <summary>
		/// Encrypt s with the 256-bit key by symmetric algorithm.
		/// </summary>
		public static string Encrypt (string s, string key)
		{
			if (key.Length != 32)
				throw new Exception ("32 character string is required as a key");

			var keyBytes = UTF8Encoding.UTF8.GetBytes (key);  // 256-bit AES key
			var bytes = UTF8Encoding.UTF8.GetBytes (s);
			var algo = new RijndaelManaged ();
			algo.Key = keyBytes;
			algo.Mode = CipherMode.ECB;  // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
			algo.Padding = PaddingMode.PKCS7;
			var encryptor = algo.CreateEncryptor ();
			var result = encryptor.TransformFinalBlock (bytes, 0, bytes.Length);
			return Convert.ToBase64String (result, 0, result.Length);
		}

		/// <summary>
		/// Decrypt the string encrypted by the provided key.
		/// </summary>
		public static string Decrypt (string encrypted, string key)
		{
			if (key.Length != 32)
				throw new Exception ("32 character string is required as a key");

			var keyBytes = UTF8Encoding.UTF8.GetBytes (key);  // 256-bit AES key
			byte[] bytes = Convert.FromBase64String (encrypted);
			var algo = new RijndaelManaged ();
			algo.Key = keyBytes;
			algo.Mode = CipherMode.ECB;  // http://msdn.microsoft.com/en-us/library/system.security.cryptography.ciphermode.aspx
			algo.Padding = PaddingMode.PKCS7;
			var decryptor = algo.CreateDecryptor ();
			var result = decryptor.TransformFinalBlock (bytes, 0, bytes.Length);
			return UTF8Encoding.UTF8.GetString (result);
		}
	}
}
