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
using System.Collections.Generic;
using System.IO;
using System.Text;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Used to encrypt/decrypt credentials to/from a binary resource. This is done to avoid storing credentials as
	/// plain text, adds an extra layer of protection.
	/// Of course this does not fully protect your secret ID, because encryption is symmetric and key is stored on
	/// the client anyway. If you really want to hide your secret ID you should store it on your own server and
	/// obtain access token for the client through your server. See "Connection" class for details. However the current
	/// solution with symmetric encryption should be fine for most apps.
	/// If the current implementation does not fit your use case or you have ideas on how to improve it - please
	/// feel free to send feedback to support@itseez3d.com
	/// </summary>
	public static class AuthUtils
	{
		/// <summary>
		/// Name of resource file where credentials are stored.
		/// </summary>
		private const string credentialsFilename = "avatar_sdk_data";

		/// <summary>
		/// Key for encryption. Feel free to change this to your liking. Needs to be 256 bits long.
		/// </summary>
		private static string GetKey ()
		{
			var key = "uTz};7c2kzk9a*pXGLp@qX$;/?,b,,,J";
			key = Convert.ToBase64String (UTF8Encoding.UTF8.GetBytes (key));
			if (key.Length > 32)
				key = key.Substring (0, 32);
			while (key.Length < 32)
				key += key [0];
			return key;
		}

		#if UNITY_EDITOR
		/// <summary>
		/// Stores the encrypted credentials in a binary resource, only used within the editor.
		/// </summary>
		public static void StoreCredentials (AccessCredentials credentials)
		{
			string[] folderTokens = { "itseez3d_misc", "auth", "resources" };
			Utils.EnsureEditorDirectoryExists (folderTokens);
			var path = Application.dataPath + "/" + string.Join ("/", folderTokens) + "/" + credentialsFilename + ".txt";

			var distraction = new StringBuilder ();
			for (int i = 0; i < 10; ++i)
				distraction.Append (Guid.NewGuid ());

			var text = string.Format ("{0} {1} {2}", credentials.clientId, credentials.clientSecret, distraction.ToString ());
			text = Convert.ToBase64String (UTF8Encoding.UTF8.GetBytes (text));
			File.WriteAllText (path, EncryptionUtils.Encrypt (text, GetKey ()));
			AssetDatabase.SaveAssets ();
			AssetDatabase.Refresh ();
		}
		#endif

		/// <summary>
		/// Load and decrypt credentials from the binary resource.
		/// </summary>
		public static AccessCredentials LoadCredentials ()
		{
			try {
				var asset = Resources.Load (credentialsFilename) as TextAsset;
				var text = EncryptionUtils.Decrypt (asset.text, GetKey ());
				text = UTF8Encoding.UTF8.GetString (Convert.FromBase64String (text));
				var tokens = text.Split (' ');
				string id = tokens [0], secret = tokens [1];
				return new AccessCredentials (id, secret);
			} catch (Exception ex) {
				Debug.LogFormat ("Could not load credentials: {0}", ex.Message);
			}

			return null;
		}
	}
}