/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
#if UNITY_ANDROID
	public class AndroidImageSupplier : AndroidJavaProxy
	{
		private bool gotResult = false;

		public AndroidImageSupplier() : base("com.itseez3d.androidplugins.ResultListener") { }

		public string FilePath { get; private set; }

		public void onResult(string filePath)
		{
			FilePath = filePath;
			gotResult = true;
			Debug.LogFormat("Opened file: {0}", filePath);
		}

		public IEnumerator GetImageFromStorageAsync()
		{
			FilePath = string.Empty;
			gotResult = false;

			AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if (unityPlayerClass == null)
			{
				Debug.LogError("Unable to create UnityPlayer java class");
				yield break;
			}
			AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaClass openFileActivityClass = new AndroidJavaClass("com.itseez3d.androidplugins.OpenFileActivity");
			openFileActivityClass.CallStatic("StartActivity", activity, this);

			while (!gotResult)
				yield return new WaitForEndOfFrame();
		}

		public IEnumerator CaptureImageFromCameraAsync()
		{
			FilePath = string.Empty;
			gotResult = false;

			AndroidJavaClass unityPlayerClass = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
			if (unityPlayerClass == null)
			{
				Debug.LogError("Unable to create UnityPlayer java class");
				yield break;
			}
			AndroidJavaObject activity = unityPlayerClass.GetStatic<AndroidJavaObject>("currentActivity");
			AndroidJavaClass cameraActivityClass = new AndroidJavaClass("com.itseez3d.androidplugins.CameraActivity");
			cameraActivityClass.CallStatic("StartActivity", activity, this);

			while (!gotResult)
				yield return new WaitForEndOfFrame();
		}
	}
#endif
}
