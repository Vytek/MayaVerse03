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
using System.IO;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdk.Core
{
	public class FileBrowser : MonoBehaviour, IPointerDownHandler, IPointerClickHandler
	{
		public System.Func<byte[], IEnumerator> fileHandler;

		public Button button;

#if UNITY_WEBGL
		[DllImport("__Internal")]
		private static extern void fileBrowserInit(string objectName, string callbackFuncName);

		[DllImport("__Internal")]
		private static extern void fileBrowserSetFocus();
#endif

		void Start()
		{
#if UNITY_WEBGL
			fileBrowserInit(gameObject.name, "FileDialogResult");
#endif
		}

		public void OnPointerDown(PointerEventData eventData)
		{
#if UNITY_WEBGL
			if (button != null && !button.interactable)
				return;
			fileBrowserSetFocus();
#endif
		}

		public void OnPointerClick(PointerEventData eventData)
		{
			if (button != null && !button.interactable)
				return;

			StartCoroutine(OpenFile());
		}

		private IEnumerator OpenFile()
		{
			string photoPath = string.Empty;
#if UNITY_EDITOR
			Utils.DisplayWarning(
				"Select .jpg or .png selfie photo",
				"Please select frontal photo of a person in .jpg or .png format. Works best on smartphone selfies (iPhone, Samsung, etc.)"
			);
			photoPath = EditorUtility.OpenFilePanelWithFilters("Select .jpg selfie photo", "", new string[] {
				"Selfie",
				"jpg,jpeg,png"
			});
#elif UNITY_ANDROID
			AndroidImageSupplier imageSupplier = new AndroidImageSupplier();
			yield return imageSupplier.GetImageFromStorageAsync();
			photoPath = imageSupplier.FilePath;
#elif UNITY_IOS
			IOSImageSupplier imageSupplier = IOSImageSupplier.Create();
			yield return imageSupplier.GetImageFromStorageAsync();
			photoPath = imageSupplier.FilePath;
#endif
			if (string.IsNullOrEmpty(photoPath))
				yield break;
			byte[] bytes = File.ReadAllBytes(photoPath);
			if (fileHandler != null)
				yield return fileHandler(bytes);
		}

		private void FileDialogResult(string fileUrl)
		{
			Debug.Log(fileUrl);
			StartCoroutine(LoadFile(fileUrl));
		}

		private IEnumerator LoadFile(string url)
		{
			var www = new WWW(url);
			yield return www;
			if (fileHandler != null)
				StartCoroutine(fileHandler(www.bytes));
		}
	}
}
