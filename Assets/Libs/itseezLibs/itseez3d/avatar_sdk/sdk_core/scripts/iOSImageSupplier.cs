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
using System.Runtime.InteropServices;
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
#if UNITY_IOS

	public class IOSImageSupplier : MonoBehaviour
	{
		[DllImport ("__Internal")]
		private unsafe static extern void getPhoto(string className, string callbackFunction);

		[DllImport ("__Internal")]
		private unsafe static extern void getPhotoFromLibrary(string className, string callbackFunction);

		private bool gotResult = false;

		private static string gameObjectName = "IOSImageSupplier";

		public string FilePath { get; private set; }

		private IOSImageSupplier()
		{
		}

		public static IOSImageSupplier Create()
		{
			GameObject gameObject = new GameObject (gameObjectName);
			return gameObject.AddComponent<IOSImageSupplier> ();
		}

		public void onResult(string filePath)
		{
			FilePath = filePath;
			gotResult = true;
			Debug.LogFormat("Opened file: {0}", filePath);
			GameObject gameObject = GameObject.Find (gameObjectName);
			if (gameObject != null)
				GameObject.Destroy (gameObject);
		}

		public IEnumerator GetImageFromStorageAsync()
		{
			FilePath = string.Empty;
			gotResult = false;

			IOSImageSupplier.getPhotoFromLibrary (gameObjectName, "onResult");

			while (!gotResult)
				yield return new WaitForEndOfFrame();
		}

		public IEnumerator CaptureImageFromCameraAsync()
		{
			FilePath = string.Empty;
			gotResult = false;

			IOSImageSupplier.getPhoto (gameObjectName, "onResult");

			while (!gotResult)
				yield return new WaitForEndOfFrame();
		}
	}
#endif
}
