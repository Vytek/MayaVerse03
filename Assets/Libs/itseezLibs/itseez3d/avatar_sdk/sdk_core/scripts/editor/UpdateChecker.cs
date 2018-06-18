/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

#if UNITY_EDITOR
using System;
using System.Security.Cryptography;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace ItSeez3D.AvatarSdk.Core.Editor
{
	/// <summary>
	/// Get the latest version number from the server. Show dialog if plugin needs to be updated.
	/// </summary>
	public class UpdateChecker
	{
		private string lastUpdatePrefKey;
		private string versionUrl;
		private System.Version currentVersion;
		private Action showUpdateWindow;

		public UpdateChecker (string preferenceKey, string url, System.Version version, Action showWarning)
		{
			lastUpdatePrefKey = preferenceKey;
			versionUrl = url;
			currentVersion = version;
			showUpdateWindow = showWarning;
		}

		public void CheckOnStartup ()
		{
			bool shouldCheck = true;
			if (EditorPrefs.HasKey (lastUpdatePrefKey)) {
				var lastCheckStr = EditorPrefs.GetString (lastUpdatePrefKey);
				var lastCheck = DateTime.Parse (lastCheckStr);
				var timeSinceLastCheck = DateTime.Now - lastCheck;
				if (timeSinceLastCheck.TotalHours < 72)
					shouldCheck = false;
			}
			if (shouldCheck)
				CheckForUpdates (automaticCheck: true);
		}

		public void CheckForUpdates (bool automaticCheck)
		{
			var r = UnityWebRequest.Get (versionUrl);
			#if UNITY_2017_2_OR_NEWER
			r.SendWebRequest();
			#else
			r.Send ();
			#endif

			EditorAsync.ProcessTask (new EditorAsync.EditorAsyncTask (
				isDone: () => r.isDone,
				onCompleted: () => OnVersionKnown (r.downloadHandler.text, automaticCheck)
			));
		}

		private void OnVersionKnown (string version, bool automaticCheck)
		{
			EditorPrefs.SetString (lastUpdatePrefKey, DateTime.Now.ToString ());

			var latestVersion = new System.Version (version);
			Debug.LogFormat ("Avatar SDK latest version is: {0}, current version is {1}", version, currentVersion);

			if (currentVersion >= latestVersion) {
				if (!automaticCheck)
					EditorUtility.DisplayDialog ("Update check", "Avatar SDK plugin is up to date!", "Ok");
			} else {
				Debug.LogFormat ("Avatar SDK version is obsolete. Update recommended.");
				showUpdateWindow ();
			}
		}
	}
}
#endif
