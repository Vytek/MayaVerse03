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
using ItSeez3D.AvatarSdk.Core;
using UnityEngine;
using UnityEngine.UI;
using System.IO;
using System;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// Utility code shared across samples.
	/// </summary>
	public static class SampleUtils
	{
		/// <summary>
		/// Display photo that is currently being used.
		/// </summary>
		public static IEnumerator DisplayPhotoPreview (string avatarCode, Image photoPreview)
		{
			var loadPhoto = CoreTools.LoadAvatarFileAsync (avatarCode, AvatarFile.PHOTO);
			yield return loadPhoto;

			Texture2D jpgTexture = new Texture2D (1, 1);
			jpgTexture.LoadImage (loadPhoto.Result);

			// make sure alpha channel is 1
			var color = photoPreview.color;
			color.a = 1;
			photoPreview.color = color;

			// create UI sprite from texture
			photoPreview.preserveAspect = true;
			photoPreview.overrideSprite = Sprite.Create (jpgTexture, new Rect (0, 0, jpgTexture.width, jpgTexture.height), Vector2.zero);
			photoPreview.gameObject.SetActive(true);
		}

		/// <summary>
		/// Very rough rescale, basically a nearest-neighbor resize
		/// </summary>
		public static Texture2D RescaleTexture(Texture2D originalTexture, int minSideSize)
		{
			int step = Math.Min(originalTexture.width, originalTexture.height) / minSideSize;
			step = step < 1 ? 1 : step;

			Texture2D scaledTexture = new Texture2D(originalTexture.width / step, originalTexture.height / step);
			for (int y = 0; y < originalTexture.height; y += step)
				for (int x = 0; x < originalTexture.width; x += step)
					scaledTexture.SetPixel(x / step, y / step, originalTexture.GetPixel(x, y));
			scaledTexture.Apply();
			return scaledTexture;
		}

#if UNITY_EDITOR
		public static byte[] LoadPhotoFromFilesystem ()
		{
			Utils.DisplayWarning (
				"Select .jpg or .png selfie photo",
				"Please select frontal photo of a person in .jpg or .png format. Works best on smartphone selfies (iPhone, Samsung, etc.)"
			);
			var photoPath = EditorUtility.OpenFilePanelWithFilters ("Select .jpg selfie photo", "", new string[] {
				"Selfie",
				"jpg,jpeg,png"
			});
			if (string.IsNullOrEmpty (photoPath))
				return null;
			return File.ReadAllBytes (photoPath);
		}
#endif

		/// <summary>
		/// Check compatibility with the current platform. Little helper just to share code between samples.
		/// </summary>
		public static bool CheckIfSupported(Text statusText, GameObject[] uiElements, SdkType sdkType)
		{
			string errorMessage = null;
			if (!CoreTools.IsPlatformSupported(sdkType, out errorMessage))
			{
				Debug.LogError(errorMessage);
				if (uiElements != null)
					foreach (var ui in uiElements)
						if (ui != null)
							ui.SetActive(false);
				statusText.GetComponent<RectTransform>().SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, Screen.height / 2);
				statusText.text = errorMessage;
				return false;
			}

			return true;
		}
	}
}

