/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

using ItSeez3D.AvatarSdk.Core;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// Script that displays haircut previews in gallery 
	/// </summary>
	public class HaircutsSelectingView : ItemsSelectingView
	{
		private IAvatarProvider avatarProvider = null;

		private string avatarCode = string.Empty;

		private const string BALD_HAIRCUT_NAME = "bald";

		public void InitItems(string avatarCode, List<string> items, IAvatarProvider avatarProvider)
		{
			AvatarSdkMgr.StopCoroutine("DisplayPreviews");
			this.avatarCode = avatarCode;
			this.avatarProvider = avatarProvider;

			InitItems(items);

			List<Toggle> previewToggles = new List<Toggle>();
			foreach (Toggle t in toggles)
			{
				Text statusText = Utils.FindSubobjectByName(t.gameObject, "StatusText").GetComponentInChildren<Text>();

				string haircutId = t.GetComponentInChildren<ToggleId>().Id;
				if (haircutId == BALD_HAIRCUT_NAME)
				{
					statusText.text = "none";
				}
				else
				{
					statusText.text = "Loading...";
					previewToggles.Add(t);
				}

				t.onValueChanged.AddListener(isOn =>
				{
					if (isOn && isShown)
						OnDoneClick();
				});
			}

			AvatarSdkMgr.SpawnCoroutine(DisplayPreviews(previewToggles));
		}

		private IEnumerator DisplayPreviews(List<Toggle> previewToggles)
		{
			foreach (Toggle t in previewToggles)
			{
				if (!t.IsDestroyed())
				{
					Text statusText = Utils.FindSubobjectByName(t.gameObject, "StatusText").GetComponentInChildren<Text>();
					string haircutId = t.GetComponentInChildren<ToggleId>().Id;
					var previewRequest = avatarProvider.GetHaircutPreviewAsync(avatarCode, haircutId);
					yield return previewRequest;
					if (previewRequest.IsError || previewRequest.Result == null)
					{
						Debug.LogErrorFormat("Unable to get preview image for haircut: {0}", haircutId);
						statusText.text = "Not found";
					}
					else
					{
						Texture2D texture = new Texture2D(1, 1);
						texture.LoadImage(previewRequest.Result);
						if (!t.IsDestroyed())
						{
							GameObject backgroundObject = Utils.FindSubobjectByName(t.gameObject, "Background");
							Image image = backgroundObject.GetComponentInChildren<Image>();
							image.sprite = Sprite.Create(texture, new Rect(0, 0, texture.width, texture.height), Vector2.zero);
							statusText.text = string.Empty;
						}
					}
				}
			}
		}
	}
}
