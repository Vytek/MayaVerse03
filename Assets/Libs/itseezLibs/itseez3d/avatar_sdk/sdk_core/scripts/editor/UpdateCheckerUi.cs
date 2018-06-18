/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

#if UNITY_EDITOR && !UNITY_WEBGL

using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdk.Core.Editor;
using UnityEditor;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Cloud.Editor
{
	[InitializeOnLoad]
	public class UpdateCheckerUi
	{
		private static UpdateChecker updateChecker;

		static UpdateCheckerUi ()
		{
			updateChecker = new UpdateChecker (
				"avatar_sdk_last_update_check",
				"https://s3.amazonaws.com/itseez3d-unity/avatar-unity-plugin/version.txt",
				CoreTools.SdkVersion,
				ShowUpdateWindow
			);

			EditorApplication.update += InitializeOnce;
		}

		private static void InitializeOnce ()
		{
			EditorApplication.update -= InitializeOnce;
			updateChecker.CheckOnStartup ();
		}

		[MenuItem ("Window/itSeez3D Avatar SDK/Check for updates")]
		public static void CheckForUpdatesMenu ()
		{
			updateChecker.CheckForUpdates (automaticCheck: false);
		}

		private static void ShowUpdateWindow ()
		{
			var msg = "There is a new version of Avatar SDK plugin! We recommend you to upgrade.";
			Utils.DisplayWarning ("Update recommended", msg);
		}
	}
}
#endif