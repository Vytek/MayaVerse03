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
using ItSeez3D.AvatarSdk.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class ResourcesSample : GettingStartedSample
	{
		#region UI elements
		public GameObject haircutsPanel;

		public GameObject blendshapesPanel;

		public ItemsSelectingView haircutsSelectingView;

		public ItemsSelectingView blendshapesSelectingView;
		#endregion


		#region public methods
		public override void OnPipelineTypeToggleChanged(bool isChecked)
		{
			base.OnPipelineTypeToggleChanged(isChecked);

			if (isChecked)
				StartCoroutine(UpdateResources());
		}
		#endregion

		#region protected methods
		/// <summary>
		/// Initializes avatar provider and requests available resources
		/// </summary>
		protected override IEnumerator Initialize()
		{
			GameObject providerContainerGameObject = GameObject.Find("AvatarProviderContainer");
			if (providerContainerGameObject != null)
			{
				avatarProvider = providerContainerGameObject.GetComponent<AvatarProviderContainer>().avatarProvider;
			}
			else
			{
				// Initialization of the IAvatarProvider may take some time. 
				// We don't want to initialize it each time when the Gallery scene is loaded.
				// So we store IAvatarProvider instance in the object that will not destroyed during navigation between the scenes (ResourceSmaple -> ModelViewer -> ResourceSample).
				providerContainerGameObject = new GameObject("AvatarProviderContainer");
				DontDestroyOnLoad(providerContainerGameObject);
				AvatarProviderContainer providerContainer = providerContainerGameObject.AddComponent<AvatarProviderContainer>();
				avatarProvider = AvatarSdkMgr.IoCContainer.Create<IAvatarProvider>();
				providerContainer.avatarProvider = avatarProvider;

				var initializeRequest = avatarProvider.InitializeAsync();
				yield return Await(initializeRequest);
				if (initializeRequest.IsError)
				{
					Debug.LogError("Avatar provider was not initialized!");
					yield break;
				}
			}

			yield return UpdateResources();
		}

		/// <summary>
		/// Generates avatar with the selected set of resources and displayed it in AvatarViewer scene
		/// </summary>
		protected override IEnumerator GenerateAndDisplayHead(byte[] photoBytes, PipelineType pipeline)
		{
			//Get selected resources
			AvatarResources avatarResources = GetSelectedResources();

			// generate avatar from the photo and get its code in the Result of request
			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes, "name", "description", pipeline, avatarResources);
			yield return Await(initializeRequest);
			string avatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(avatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(avatarCode);
			yield return Await(calculateRequest);

			AvatarViewer.SetSceneParams(new AvatarViewer.SceneParams()
			{
				avatarCode = avatarCode,
				showSettings = false,
				sceneToReturn = SceneManager.GetActiveScene().name,
				avatarProvider = avatarProvider,
				useAnimations = false
			});
			SceneManager.LoadScene(Scenes.GetSceneName(SceneType.AVATAR_VIEWER));
		}

		protected override void SetControlsInteractable(bool interactable)
		{
			base.SetControlsInteractable(interactable);

			foreach (Selectable c in haircutsPanel.GetComponentsInChildren<Selectable>())
				c.interactable = interactable;

			foreach (Selectable c in blendshapesPanel.GetComponentsInChildren<Selectable>())
				c.interactable = interactable;
		}

		protected override void OnDestroy()
		{
			//Do nothing. 
			//We don't want to destroy avatarProvider here. It will be done by AvatarProviderContainer
		}

		protected IEnumerator UpdateResources()
		{
			SetControlsInteractable(false);

			// Get all available resources
			var allResourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.ALL, pipelineType);
			// Get default resources
			var defaultResourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.DEFAULT, pipelineType);
			yield return Await(allResourcesRequest, defaultResourcesRequest);

			if (allResourcesRequest.IsError || defaultResourcesRequest.IsError)
			{
				Debug.LogError("Unable to get resources list");
				haircutsSelectingView.InitItems(new List<string>());
				blendshapesSelectingView.InitItems(new List<string>());
			}
			else
			{
				AvatarResources allResources = allResourcesRequest.Result;
				AvatarResources defaultResources = defaultResourcesRequest.Result;

				haircutsSelectingView.InitItems(allResources.haircuts);
				haircutsSelectingView.Show(defaultResources.haircuts, null);

				blendshapesSelectingView.InitItems(allResources.blendshapes);
				blendshapesSelectingView.Show(defaultResources.blendshapes, null);
			}

			SetControlsInteractable(true);
		}

		/// <summary>
		/// Forms selected resources lists
		/// </summary>
		protected AvatarResources GetSelectedResources()
		{
			AvatarResources resources = AvatarResources.Empty;
			resources.haircuts = haircutsSelectingView.CurrentSelection;
			resources.blendshapes = blendshapesSelectingView.CurrentSelection;
			return resources;
		}
		#endregion
	}
}
