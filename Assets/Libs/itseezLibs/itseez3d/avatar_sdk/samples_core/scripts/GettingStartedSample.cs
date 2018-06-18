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
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using System.IO;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class GettingStartedSample : MonoBehaviour
	{
		public SdkType sdkType;

		// Test data
		public TextAsset[] testPhotos;

		#region UI
		public Text progressText;
		public Button[] buttons;
		public Image photoPreview;
		#endregion

		protected FileBrowser fileBrowser = null;

		// Instance of IAvatarProvider. Do not forget to call Dispose upon MonoBehaviour destruction.
		protected IAvatarProvider avatarProvider = null;

		protected virtual void Start()
		{
			var ui = buttons.Select(b => b.gameObject).ToArray();
			if (!SampleUtils.CheckIfSupported(progressText, ui, sdkType))
				return;

			// first of all, initialize the SDK
			AvatarSdkMgr.Init();

			avatarProvider = CoreTools.CreateAvatarProvider(sdkType);
			StartCoroutine(Initialize());

			// Anti-aliasing is required for hair shader, otherwise nice transparent texture won't work.
			// Another option is to use cutout shader, but the look with this shader isn't that great.
#if UNITY_STANDALONE_WIN || UNITY_EDITOR || UNITY_EDITOR
			QualitySettings.antiAliasing = 8;
#else
			QualitySettings.antiAliasing = 4;
#endif
			foreach (var b in buttons)
			{
#if UNITY_EDITOR || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS

				if (b.name.Contains("UserPhoto"))
				{
					b.gameObject.SetActive(true);
					fileBrowser = b.GetComponentInChildren<FileBrowser>();
					if (fileBrowser != null) {
						fileBrowser.fileHandler = GenerateAvatarFunc;
					}
				}
#endif
#if UNITY_ANDROID || UNITY_IOS
				if (b.name.Contains("CameraPhoto")) {
					b.gameObject.SetActive(true);
				}
#endif
			}
		}

		/// <summary>
		/// Initialize avatar provider
		/// </summary>
		private IEnumerator Initialize()
		{
			yield return Await(avatarProvider.InitializeAsync());
		}

		/// <summary>
		/// Button click handler.
		/// Loads one of the predefined photos from resources.
		/// </summary>
		public void GenerateRandomAvatar()
		{
			// Load random sample photo from the assets. Here you may replace it with your own photo.
			var testPhotoIdx = UnityEngine.Random.Range(0, testPhotos.Length);
			var testPhoto = testPhotos[testPhotoIdx];
			StartCoroutine(GenerateAvatarFunc(testPhoto.bytes));
		}

		/// <summary>
		/// Button click handler.
		/// Starts coroutine to generate avatar from camera's photo.
		/// </summary>
		public void GenerateAvatarFromCameraPhoto()
		{
			StartCoroutine(GenerateAvatarFromCameraPhotoAsync());
		}

		/// <summary>
		/// Launches camera application on mobile platforms, takes photo and generates avatar from it.
		/// </summary>
		private IEnumerator GenerateAvatarFromCameraPhotoAsync()
		{
			string photoPath = string.Empty;
#if UNITY_ANDROID
			AndroidImageSupplier imageSupplier = new AndroidImageSupplier();
			yield return imageSupplier.CaptureImageFromCameraAsync();
			photoPath = imageSupplier.FilePath;
#elif UNITY_IOS
			IOSImageSupplier imageSupplier = IOSImageSupplier.Create();
			yield return imageSupplier.CaptureImageFromCameraAsync();
			photoPath = imageSupplier.FilePath;
#endif
			if (string.IsNullOrEmpty(photoPath))
				yield break;
			byte[] bytes = File.ReadAllBytes(photoPath);
			yield return GenerateAvatarFunc(bytes);
		}

		/// <summary>
		/// Destroy the existing avatar in the scene. Disable the buttons.
		/// Wait until coroutine finishes and then enable buttons again.
		/// </summary>
		protected virtual IEnumerator GenerateAvatarFunc(byte[] photoBytes)
		{
			var avatarObject = GameObject.Find("ItSeez3D Avatar");
			Destroy(avatarObject);
			SetButtonsInteractable(false);
			photoPreview.gameObject.SetActive(false);
			yield return StartCoroutine(GenerateAndDisplayHead(photoBytes));
			SetButtonsInteractable(true);
		}

		/// <summary>
		/// Helper function that allows to yield on multiple async requests in a coroutine.
		/// It also tracks progress on the current request(s) and updates it in UI.
		/// </summary>
		protected IEnumerator Await(params AsyncRequest[] requests)
		{
			foreach (var r in requests)
				while (!r.IsDone)
				{
					// yield null to wait until next frame (to avoid blocking the main thread)
					yield return null;

					// This function will throw on any error. Such primitive error handling only provided as
					// an example, the production app probably should be more clever about it.
					if (r.IsError)
					{
						Debug.LogError(r.ErrorMessage);
						throw new Exception(r.ErrorMessage);
					}

					// Each requests may or may not contain "subrequests" - the asynchronous subtasks needed to
					// complete the request. The progress for the requests can be tracked overall, as well as for
					// every subtask. The code below shows how to recursively iterate over current subtasks
					// to display progress for them.
					var progress = new List<string>();
					AsyncRequest request = r;
					while (request != null)
					{
						progress.Add(string.Format("{0}: {1}%", request.State, request.ProgressPercent.ToString("0.0")));
						request = request.CurrentSubrequest;
					}
					progressText.text = string.Join("\n", progress.ToArray());
				}
		}

		/// <summary>
		/// To make Getting Started sample as simple as possible all code required for creating and
		/// displaying an avatar is placed here in a single function. This function is also a good example of how to
		/// chain asynchronous requests, just like in traditional sequential code.
		/// </summary>
		protected virtual IEnumerator GenerateAndDisplayHead(byte[] photoBytes)
		{
			// generate avatar from the photo and get its code in the Result of request
			var initializeRequest = avatarProvider.InitializeAvatarAsync(photoBytes);
			yield return Await(initializeRequest);
			string avatarCode = initializeRequest.Result;

			StartCoroutine(SampleUtils.DisplayPhotoPreview(avatarCode, photoPreview));

			var calculateRequest = avatarProvider.StartAndAwaitAvatarCalculationAsync(avatarCode);
			yield return Await(calculateRequest);

			// with known avatar code we can get TexturedMesh for head in order to show it further
			var avatarHeadRequest = avatarProvider.GetHeadMeshAsync(avatarCode, false);
			yield return Await(avatarHeadRequest);
			TexturedMesh headTexturedMesh = avatarHeadRequest.Result;

			// get identities of all haircuts available for the generated avatar
			var haircutsIdRequest = avatarProvider.GetHaircutsIdAsync(avatarCode);
			yield return Await(haircutsIdRequest);

			// randomly select a haircut
			var haircuts = haircutsIdRequest.Result;
			var haircutIdx = UnityEngine.Random.Range(0, haircuts.Length);
			var haircut = haircuts[haircutIdx];

			// load TexturedMesh for the chosen haircut 
			var haircutRequest = avatarProvider.GetHaircutMeshAsync(avatarCode, haircut);
			yield return Await(haircutRequest);
			TexturedMesh haircutTexturedMesh = haircutRequest.Result;

			DisplayHead(headTexturedMesh, haircutTexturedMesh);
		}

		/// <summary>
		/// Displays head mesh and harcut on the scene
		/// </summary>
		protected virtual void DisplayHead(TexturedMesh headMesh, TexturedMesh haircutMesh)
		{
			// create parent avatar object in a scene, attach a script to it to allow rotation by mouse
			var avatarObject = new GameObject("ItSeez3D Avatar");
			avatarObject.AddComponent<RotateByMouse>();

			// create head object in the scene
			Debug.LogFormat("Generating Unity mesh object for head...");
			var headObject = new GameObject("HeadObject");
			headObject.AddComponent<MeshFilter>().mesh = headMesh.mesh;
			var headMeshRenderer = headObject.AddComponent<MeshRenderer>();
			var headMaterial = new Material(Shader.Find("AvatarUnlitShader"));
			headMaterial.mainTexture = headMesh.texture;
			headMeshRenderer.material = headMaterial;
			headObject.transform.SetParent(avatarObject.transform);

			if (haircutMesh != null)
			{
				// create haircut object in the scene
				var haircutObject = new GameObject("HaircutObject");
				haircutObject.AddComponent<MeshFilter>().mesh = haircutMesh.mesh;
				var haircutMeshRenderer = haircutObject.AddComponent<MeshRenderer>();
				var haircutMaterial = new Material(Shader.Find("AvatarUnlitHairShader"));
				haircutMaterial.mainTexture = haircutMesh.texture;
				haircutMeshRenderer.material = haircutMaterial;
				haircutObject.transform.SetParent(avatarObject.transform);
			}
		}

		/// <summary>
		/// Allows to change buttons interactability.
		/// </summary>
		protected void SetButtonsInteractable(bool interactable)
		{
			foreach (var b in buttons)
				b.interactable = interactable;
		}

		/// <summary>
		/// This is crucial! Don't forget to call Dispose for the avatar provider, or use "using" keyword.
		/// </summary>
		protected void OnDestroy()
		{
			Debug.LogFormat("Calling avatar provider dispose method!");
			avatarProvider.Dispose();
		}
	}
}
