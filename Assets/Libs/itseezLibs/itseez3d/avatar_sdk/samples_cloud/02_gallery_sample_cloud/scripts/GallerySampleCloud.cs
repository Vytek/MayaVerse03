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
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	public class GallerySampleCloud : GallerySample
	{
		// Extended GalleryAvatar class to store AvatarData.
		// Storing AvatarData allows to reduce the number of requests to the server.
		protected class GalleryAvatarCloud : GalleryAvatar
		{
			public AvatarData avatarData;
		}

		// avatar instance that is being edited, valid only when edit window is present on the screen
		private AvatarData avatarToEdit;

		public GallerySampleCloud()
		{
			sdkType = SdkType.Cloud;
		}

		#region overrided base methods

		/// <summary>
		/// Override this method just to show how the connection authorization in the Cloud SDK can be customized
		/// </summary>
		/// <returns></returns>
		protected override AsyncRequest InitializeAvatarProviderAsync()
		{
			#if CUSTOM_PLAYER_UID_SAMPLE
			// You might want to link playerUID e.g. to account in your game, so player with the same account sees
			// the same avatars on all devices. In order to achieve this you should specify PlayerUID before
			// calling connection.AuthorizeAsync()
			Connection connection = (avatarProvider as CloudAvatarProvider).Connection;
			connection.PlayerUID = myPlayerUID;
			#endif

			#if EXTERNAL_AUTH_SAMPLE
			// If you really don't want to store secret ID (albeit encrypted) on the client, you can obtain
			// auth token via your own server and authorize connection using the following method:
			Connection connection = (avatarProvider as CloudAvatarProvider).Connection;
			connection.AuthorizeWithCredentials (myTokenType, myAccessToken, myPlayerUID);
			return new AsyncRequest() { IsDone = true };
			#endif

			return avatarProvider.InitializeAsync();
		}

		protected override AsyncRequest<GalleryAvatar[]> GetAllAvatarsAsync(int maxItems)
		{
			var request = new AsyncRequest<GalleryAvatar[]>(AvatarSdkMgr.Str(Strings.GettingAvatarState));
			AvatarSdkMgr.SpawnCoroutine(GetAllAvatarsFunc(maxItems, request));
			return request;
		}

		protected override void InitAvatarPreview(AvatarPreview preview, string avatarCode, GalleryAvatarState avatarState)
		{
			preview.InitPreview(this, avatarCode, avatarState, true);

			GalleryAvatarCloud avatar = loadedAvatars.FirstOrDefault(a => string.Compare(a.code, avatarCode) == 0) as GalleryAvatarCloud;
			preview.UpdateAvatarName(avatar.avatarData.name);
		}

		public override void OnEditAvatar(string avatarCode)
		{
			StartCoroutine(ShowEditPanel(avatarCode));
		}

		public override void OnEditConfirm()
		{
			StartCoroutine(EditAvatar());
		}
		#endregion

		/// <summary>
		/// Requests all created avatars that created by current user and determinates states for them
		/// </summary>
		private IEnumerator GetAllAvatarsFunc(int maxItems, AsyncRequest<GalleryAvatar[]> request)
		{
			Connection connection = (avatarProvider as CloudAvatarProvider).Connection;
			var avatarsRequest = connection.GetAvatarsAsync(maxItems);
			yield return Await(avatarsRequest, null);
			if (avatarsRequest.IsError)
				yield break;

			GalleryAvatar[] avatars = new GalleryAvatar[avatarsRequest.Result.Length];
			for (int i=0; i<avatars.Length; i++)
			{
				AvatarData avatarData = avatarsRequest.Result[i];
				avatars[i] = new GalleryAvatarCloud() { code = avatarData.code, state = GetAvatarState(avatarData), avatarData = avatarData };
			}

			request.Result = avatars;
			request.IsDone = true;
		}

		private GalleryAvatarState GetAvatarState(AvatarData avatarData)
		{
			GalleryAvatarState avatarState = GalleryAvatarState.UNKNOWN;
			// check if calculation failed on the server
			if (Strings.BadFinalStates.Contains(avatarData.status))
				avatarState = GalleryAvatarState.FAILED;
			else
			{
				if (Strings.GoodFinalStates.Contains(avatarData.status))
					avatarState = GalleryAvatarState.COMPLETED;
				else
					// not failed server status, but not completed either - this means avatar is still on the server
					avatarState = GalleryAvatarState.GENERATING;
			}
			return avatarState;
		}

		/// <summary>
		/// Shows panel to edit name and description of the avatar
		/// </summary>
		private IEnumerator ShowEditPanel(string avatarCode)
		{
			CloudAvatarProvider cloudAvatarProvider = avatarProvider as CloudAvatarProvider;
			var avatarRequest = cloudAvatarProvider.Connection.GetAvatarAsync(avatarCode);
			yield return avatarRequest;
			if (avatarRequest.IsError)
				yield break;

			avatarToEdit = avatarRequest.Result;
			var avatarEdit = editPanel.GetComponent<AvatarEdit>();
			avatarEdit.nameField.text = avatarToEdit.name;
			avatarEdit.descriptionField.text = avatarToEdit.description;
			editPanel.SetActive(true);
		}

		/// <summary>
		/// Applies the changes in avatar name and description. Updates this avatar on the server.
		/// </summary>
		/// <returns></returns>
		private IEnumerator EditAvatar()
		{
			CloudAvatarProvider cloudAvatarProvider = avatarProvider as CloudAvatarProvider;
			var avatarEdit = editPanel.GetComponent<AvatarEdit>();
			yield return Await(
				cloudAvatarProvider.Connection.EditAvatarAsync(avatarToEdit, avatarEdit.nameField.text, avatarEdit.descriptionField.text),
				avatarToEdit.code
			);
			yield return UpdateAvatarList();
			editPanel.SetActive(false);
			avatarToEdit = null;
		}
	}
}