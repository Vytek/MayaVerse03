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
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// This sample demonstrates how to use the level-of-details functionality.
	/// </summary>
	public class LODSample : GettingStartedSample
	{
		public Text detailsLevelText = null;

		public GameObject detailsLevelControls = null; 

		private int currentDetailsLevel = 0;
		private string currentAvatarCode = string.Empty;

		public void PrevDetailedMeshClick()
		{
			StartCoroutine(ChangeMeshDetailsLevel(currentDetailsLevel - 1));
		}

		public void NextDetailedMeshClick()
		{
			StartCoroutine(ChangeMeshDetailsLevel(currentDetailsLevel + 1));
		}

		protected override IEnumerator GenerateAvatarFunc(byte[] photoBytes)
		{
			detailsLevelControls.SetActive(false);
			yield return base.GenerateAvatarFunc(photoBytes);
			detailsLevelControls.SetActive(true);
		}

		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			// We don't need blendshapes nor haircuts. So create an empty avatar resources set
			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, AvatarResources.Empty);
			yield return Await(initializeRequest);
			currentAvatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(currentAvatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(currentAvatarCode);
			yield return Await(calculateRequest);

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(currentAvatarCode, false, currentDetailsLevel);
			yield return Await(avatarHeadRequest);

			DisplayHead(avatarHeadRequest.Result, null);
			detailsLevelText.text = string.Format("Triangles count:\n{0}", avatarHeadRequest.Result.mesh.triangles.Length / 3);
		}

		private IEnumerator ChangeMeshDetailsLevel(int newDetailsLevel)
		{
			if (newDetailsLevel < 0 || newDetailsLevel > 4)
				yield break;

			currentDetailsLevel = newDetailsLevel;
			SetControlsInteractable(false);
			yield return ChangeMeshResolution(currentAvatarCode, currentDetailsLevel);
			SetControlsInteractable(true);
		}

		private IEnumerator ChangeMeshResolution(string avatarCode, int detailsLevel)
		{
			var headObject = GameObject.Find("HeadObject");
			if (headObject == null)
				yield break;

			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(avatarCode, false, detailsLevel);
			yield return Await(avatarHeadRequest);

			SkinnedMeshRenderer meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			meshRenderer.sharedMesh = avatarHeadRequest.Result.mesh;
			detailsLevelText.text = string.Format("Triangles count:\n{0}", meshRenderer.sharedMesh.triangles.Length / 3);
		}
	}
}
