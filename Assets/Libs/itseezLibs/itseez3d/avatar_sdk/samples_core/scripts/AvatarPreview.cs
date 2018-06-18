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

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// Helper class for avatar previews in UI.
	/// </summary>
	public class AvatarPreview : MonoBehaviour
	{
		// id of the avatar this UI item corresponds to
		string avatarCode;

		// stores link to parent gallery manager
		GallerySample gallery;

		// indicates whether the avatar name can be edited
		bool isCustomizationEnabled = false;

		#region UI

		public Text avatarName, code, status, progress;
		public UnityEngine.UI.Image image;
		public Button showButton, editButton, deleteButton;

		#endregion

		/// <summary>
		/// Loads the preview image from the disk asynchronously, resizes it to a smaller size and displays in UI.
		/// </summary>
		private IEnumerator UpdateImageAsync ()
		{
			var loadPhoto = CoreTools.LoadAvatarFileAsync (avatarCode, AvatarFile.PHOTO);
			yield return loadPhoto;
			if (loadPhoto.IsError) {
				Debug.Log ("Could not load photo (probably original photo was not saved for this avatar)");
				yield break;
			}

			// parse jpg bytes into texture
			Texture2D jpgTexture = new Texture2D (1, 1);
			jpgTexture.LoadImage (loadPhoto.Result);

			// rescale to reduce load time and memory usage
			Texture2D previewTexture = SampleUtils.RescaleTexture(jpgTexture, 300);

			// do not keep original hires texture in the memory (hopefully GC will collect it)
			Destroy (jpgTexture);
			jpgTexture = null;

			// make sure alpha channel is 1
			var color = image.color;
			color.a = 1;
			image.color = color;

			// create UI sprite from texture
			image.preserveAspect = true;
			image.overrideSprite = Sprite.Create (previewTexture, new Rect (0, 0, previewTexture.width, previewTexture.height), Vector2.zero);
		}

		public void InitPreview (GallerySample g, string code, GalleryAvatarState state, bool customizationEnabled)
		{
			Debug.LogFormat ("Initializing preview...");

			avatarCode = code;
			gallery = g;
			isCustomizationEnabled = customizationEnabled;
			AvatarSdkMgr.SpawnCoroutine (UpdateImageAsync ());

			editButton.gameObject.SetActive(customizationEnabled);
			avatarName.gameObject.SetActive(customizationEnabled);

			UpdatePreview (avatarCode, state);
		}

		/// <summary>
		/// Update fields in UI when avatar progress is changed.
		/// </summary>
		public void UpdatePreview (string avatarCode, GalleryAvatarState state)
		{
			code.text = string.Format ("Code: {0}...", avatarCode.Substring (0, 24));
			status.text = string.Format ("State: {0}", state);

			editButton.gameObject.SetActive (false);
			showButton.gameObject.SetActive (false);
			deleteButton.gameObject.SetActive(state != GalleryAvatarState.GENERATING);

			if (state == GalleryAvatarState.COMPLETED)
			{
				if (isCustomizationEnabled)
					editButton.gameObject.SetActive(true);
				showButton.gameObject.SetActive(true);
			}

			if (state != GalleryAvatarState.GENERATING)
				progress.text = string.Empty;
		}

		public void UpdateAvatarName(string name)
		{
			avatarName.text = string.Format("Name: {0}", name);
		}

		/// <summary>
		/// Help GC to clean up memory (mostly preview sprites).
		/// </summary>
		public void CleanUp ()
		{
			image.overrideSprite = image.sprite = null;
			Destroy (image);
		}

		public void UpdateProgress (string progressStr)
		{
			progress.text = progressStr;
		}

		#region Button click handlers

		public void OnShow ()
		{
			gallery.OnShowAvatar (avatarCode);
		}

		public void OnEdit ()
		{
			gallery.OnEditAvatar (avatarCode);
		}

		public void OnDelete ()
		{
			gallery.OnDeleteAvatar (avatarCode);
		}

		#endregion
	}
}