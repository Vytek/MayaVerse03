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
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using UnityEngine.Networking;
using System.Runtime.InteropServices;
using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using System.Text;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	/// <summary>
	/// Avatar states for a simple "state machine" implemented within WebglSample class.
	/// </summary>
	public enum WebglAvatarState
	{
		// avatar image is being uploaded to the server
		UPLOADING,
		// avatar is being calculated on the server (all server states like Queued and Computing go here)
		CALCULATING_IN_CLOUD,
		// downloading results from server, avatar should be in "Completed" state on the server
		DOWNLOADING,
		// finished downloading the results, avatar is ready to be displayed in the scene
		FINISHED,
		// Calculations failed on the server, cannot download results and display avatar.
		// Make sure your photo is decent quality and resolution and contains a human face. Works best on selfies.
		FAILED,
	}

	public class WebglSample : MonoBehaviour
	{
		public GameObject sampleImagesPanel;
		public FileBrowser fileBrowser;
		public Image image;
		public Text statusText, progressText, urlUploadingStatusText;
		public Button uploadButton, showButton, browseButton, urlButton;
		public Button feedbackButton;
		public GameObject browsePanel;
		public InputField urlInput;

		private bool controlsEnabled = true;

		private Connection connection = null;

		// these variables should be static to maintain the previous state of the scene
		// in case the scene was reloaded
		private static byte[] selectedImageBytes = null;
		private static CloudAvatarProvider avatarProvider = null;
		private static AvatarData createdAvatar = null;
		private static WebglAvatarState avatarState;
		private static AsyncRequest<AvatarData> faceAvatarRequest = null;

#if UNITY_WEBGL
		private readonly string urlProxy = "https://accounts.avatarsdk.com/imgp/";

		[DllImport("__Internal")]
		private static extern void showPrompt(string message, string objectName, string callbackFuncName);
#endif

		void Start()
		{
#if !UNITY_EDITOR
#if UNITY_WEBGL
			urlInput.gameObject.SetActive(false);
			urlButton.gameObject.SetActive(true);
			HorizontalLayoutGroup browsePanelLayout = browsePanel.GetComponentInChildren<HorizontalLayoutGroup>();
			browsePanelLayout.childControlWidth = true;
#else
			browsePanel.SetActive(false);
#endif
#endif

			fileBrowser.fileHandler = HandleUploadedImage;
			StartCoroutine(Initialize());
		}

		public void OnEnterURLEnded(string value)
		{
			Debug.LogFormat("Entered: {0}", value);
			if (string.IsNullOrEmpty(value))
				return;

			StartCoroutine(UploadImageByUrl(value));
		}

		public void OnUploadButtonClick()
		{
			StartCoroutine(CreateNewAvatar());
		}

		public void OnShowButtonClick()
		{
			AvatarViewer.SetSceneParams(new AvatarViewer.SceneParams()
			{
				avatarCode = createdAvatar.code,
				showSettings = false,
				sceneToReturn = SceneManager.GetActiveScene().name,
				avatarProvider = WebglSample.avatarProvider,
				faceAvatarRequest = faceAvatarRequest
			});
			SceneManager.LoadScene(Scenes.GetSceneName(SceneType.AVATAR_VIEWER));
		}

		public void OnEnterUrlClick()
		{
#if UNITY_WEBGL
			showPrompt("Enter url", gameObject.name, "OnEnterURLEnded");
#endif
		}

		private IEnumerator Initialize()
		{
			if (!AvatarSdkMgr.IsInitialized)
				AvatarSdkMgr.Init(stringMgr: new DefaultStringManager(), storage: new DefaultPersistentStorage(), sdkType: SdkType.Cloud);

			if (avatarProvider == null)
				avatarProvider = new CloudAvatarProvider();
			connection = avatarProvider.Connection;

			var imageItems = sampleImagesPanel.GetComponentsInChildren<ImageItem>();
			foreach (ImageItem item in imageItems)
				item.imageSelectedHandler = HandleSelectedImage;

			if (createdAvatar != null)
			{
				UpdateSelectedImage(selectedImageBytes);
				UpdateAvatarState(avatarState, PipelineType.HEAD);
			}
			image.gameObject.SetActive(true);

			// initialize provider
			if (!avatarProvider.Connection.IsAuthorized)
			{
				yield return avatarProvider.InitializeAsync();
				if (!avatarProvider.Connection.IsAuthorized)
				{
					Debug.LogError("Authentication failed!");
					yield break;
				}
			}
		}

		private void UpdateSelectedImage(byte[] bytes)
		{
			ResetControlsToDefaultState();

			selectedImageBytes = bytes;

			Texture2D jpgTexture = new Texture2D(1, 1);
			jpgTexture.LoadImage(selectedImageBytes);

			Texture2D previewTexture = SampleUtils.RescaleTexture(jpgTexture, 300);
			Destroy(jpgTexture);
			jpgTexture = null;

			var color = image.color;
			color.a = 1;
			image.color = color;

			image.preserveAspect = true;
			image.overrideSprite = Sprite.Create(previewTexture, new Rect(0, 0, previewTexture.width, previewTexture.height), Vector2.zero);
		}

		private void HandleSelectedImage(byte[] imageBytes)
		{
			if (controlsEnabled)
				UpdateSelectedImage(imageBytes);
		}

		private IEnumerator HandleUploadedImage(byte[] imageBytes)
		{
			UpdateSelectedImage(imageBytes);
			yield return new WaitForEndOfFrame();
		}

		private IEnumerator UploadImageByUrl(string url)
		{
			if (!Uri.IsWellFormedUriString(url, UriKind.Absolute))
			{
				string msg = "Invalid URL";
				Debug.LogError(msg);
				ShowImageUploadingStatus(msg);
				yield break;
			}

			ChangeControlsState(false);

#if !UNITY_EDITOR && UNITY_WEBGL
			// In webgl we need to use proxy due to security restrictions
			string encodedUrl = Base64.Encode(Encoding.UTF8.GetBytes(url), true);
			url = urlProxy + encodedUrl;
#endif

#if UNITY_2017_2_OR_NEWER
			UnityWebRequest webRequest = UnityWebRequestTexture.GetTexture(url);
			webRequest.SendWebRequest();
#else
			UnityWebRequest webRequest = UnityWebRequest.GetTexture (url);
			webRequest.Send();
#endif

			do
			{
				yield return null;
				string statusMessage = string.Format("Downloading image: {0}%", (webRequest.downloadProgress * 100).ToString("0.0"));
				ShowImageUploadingStatus(statusMessage);
			} while (!webRequest.isDone);

			ChangeControlsState(true);
			urlInput.text = string.Empty;

#if UNITY_2017_1_OR_NEWER
			if (webRequest.isNetworkError)
#else
			if (webRequest.isError)
#endif
			{
				Debug.LogErrorFormat("Unable to upload image: {0}, Code: {1}!", webRequest.error, webRequest.responseCode);
				ShowImageUploadingStatus("Unable to get image by URL!");
				yield break;
			}

			// www.texture contains red question-mark (8x8) if no image was loaded
			// we need do detect such case and handle it as error 
			DownloadHandlerTexture texturehandler = ((DownloadHandlerTexture)webRequest.downloadHandler);
			if (texturehandler.texture == null || (texturehandler.texture.width == 8 && texturehandler.texture.height == 8))
			{
				Debug.LogErrorFormat("Unable to upload image2: {0}, Code: {1}!", webRequest.error, webRequest.responseCode);
				ShowImageUploadingStatus("Unable to get image by URL!");
				yield break;
			}

			HideImageUploadingStatus();
			UpdateSelectedImage(webRequest.downloadHandler.data);
		}

		private IEnumerator CreateNewAvatar()
		{
			ChangeControlsState(false);
			createdAvatar = null;

			// Face avatar is being calculated in background
			faceAvatarRequest = GenerateAvatarAsync(selectedImageBytes, PipelineType.FACE);
			var headAvatarRequest = GenerateAvatarAsync(selectedImageBytes, PipelineType.HEAD);

			yield return headAvatarRequest;

			ChangeControlsState(true);

			if (!headAvatarRequest.IsError && avatarState == WebglAvatarState.FINISHED)
			{
				createdAvatar = headAvatarRequest.Result;
				OnShowButtonClick();
			}
		}

		private AsyncRequest<AvatarData> GenerateAvatarAsync(byte[] selectedImageBytes, PipelineType pipelineType)
		{
			var request = new AsyncRequest<AvatarData>();
			AvatarSdkMgr.SpawnCoroutine(GenerateAvatarFunc(selectedImageBytes, pipelineType, request));
			return request;
		}

		private AsyncRequest DownloadAvatarAsync(AvatarData avatar, PipelineType pipelineType)
		{
			var request = new AsyncRequest();
			AvatarSdkMgr.SpawnCoroutine(DownloadAvatarFunc(avatar, pipelineType, request));
			return request;
		}

		private IEnumerator GenerateAvatarFunc(byte[] selectedImageBytes, PipelineType pipelineType, AsyncRequest<AvatarData> request)
		{
			UpdateAvatarState(WebglAvatarState.UPLOADING, pipelineType);

			var defaultResourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.DEFAULT, pipelineType);
			yield return Await(defaultResourcesRequest, pipelineType);

			// Generate all haircuts and default blendshapes to play animations
			var allResourcesRequest = avatarProvider.ResourceManager.GetResourcesAsync(AvatarResourcesSubset.ALL, pipelineType);
			yield return Await(allResourcesRequest, pipelineType);
			if (defaultResourcesRequest.IsError || allResourcesRequest.IsError)
			{
				string msg = "Unable to get resources list";
				Debug.LogError(msg);
				UpdateAvatarState(WebglAvatarState.FAILED, pipelineType);
				request.SetError(msg);
				yield break;
			}
			AvatarResources resources = allResourcesRequest.Result;
			resources.blendshapes = defaultResourcesRequest.Result.blendshapes;

			var createAvatar = connection.CreateAvatarWithPhotoAsync("test_avatar", "test_description", selectedImageBytes, false, pipelineType, resources);
			yield return Await(createAvatar, pipelineType);
			if (createAvatar.IsError)
			{
				Debug.LogError(createAvatar.ErrorMessage);
				UpdateAvatarState(WebglAvatarState.FAILED, pipelineType);
				request.SetError(createAvatar.ErrorMessage);
				yield break;
			}

			var avatar = createAvatar.Result;
			var savePhoto = CoreTools.SaveAvatarFileAsync(selectedImageBytes, avatar.code, AvatarFile.PHOTO);
			yield return savePhoto;

			var savePipeline = CoreTools.SaveAvatarFileAsync(Encoding.ASCII.GetBytes(pipelineType.GetPipelineTypeName()), avatar.code, AvatarFile.PIPELINE_INFO);
			yield return savePipeline;

			if (savePhoto.IsError)
			{
				Debug.LogError(savePhoto.ErrorMessage);
				UpdateAvatarState(WebglAvatarState.FAILED, pipelineType);
				request.SetError(savePhoto.ErrorMessage);
				yield break;
			}

			UpdateAvatarState(WebglAvatarState.CALCULATING_IN_CLOUD, pipelineType);

			var awaitCalculations = connection.AwaitAvatarCalculationsAsync(avatar);
			yield return Await(awaitCalculations, pipelineType);

			if (awaitCalculations.IsError)
			{
				Debug.LogError(awaitCalculations.ErrorMessage);
				UpdateAvatarState(WebglAvatarState.FAILED, pipelineType);
				request.SetError(awaitCalculations.ErrorMessage);
				yield break;
			}

			AvatarData avatarData = awaitCalculations.Result;
			UpdateAvatarState(WebglAvatarState.DOWNLOADING, pipelineType);
			var downloadRequest = DownloadAvatarAsync(avatarData, pipelineType);
			yield return downloadRequest;

			if (downloadRequest.IsError)
			{
				Debug.LogError(downloadRequest.ErrorMessage);
				UpdateAvatarState(WebglAvatarState.FAILED, pipelineType);
				request.SetError(downloadRequest.ErrorMessage);
				yield break;
			}

			UpdateAvatarState(WebglAvatarState.FINISHED, pipelineType);
			request.Result = avatarData;
			request.IsDone = true;
		}

		private IEnumerator DownloadAvatarFunc(AvatarData avatar, PipelineType pipelineType, AsyncRequest request)
		{
			//Update avatar info
			int retryCount = 3;
			bool gotAvatar = false;
			while (!gotAvatar)
			{
				if (retryCount == 0)
				{
					request.SetError("Unable to download avatar");
					yield break;
				}

				var updateAvatar = connection.GetAvatarAsync(avatar.code);
				yield return Await(updateAvatar, pipelineType);
				if (!updateAvatar.IsError)
				{
					gotAvatar = true;
					avatar = updateAvatar.Result;
				}
				retryCount--;
			}

			// download avatar files
			retryCount = 3;
			bool isDownloaded = false;
			while (!isDownloaded)
			{
				if (retryCount == 0)
				{
					request.SetError("Unable to download avatar");
					yield break;
				}

				var downloadAvatar = avatarProvider.DownloadAndSaveAvatarModelAsync(avatar, pipelineType == PipelineType.FACE, true);
				yield return Await(downloadAvatar, pipelineType);
				isDownloaded = !downloadAvatar.IsError;
				if (downloadAvatar.IsError)
					yield return new WaitForSeconds(3);

				retryCount--;
			}

			request.IsDone = true;
		}

		private static string StatePretty(WebglAvatarState state)
		{
			switch (state)
			{
				case WebglAvatarState.UPLOADING:
					return "uploading photo to the server";
				case WebglAvatarState.CALCULATING_IN_CLOUD:
					return "deep learning inference";
				case WebglAvatarState.DOWNLOADING:
					return "downloading avatar files";
				case WebglAvatarState.FINISHED:
					return "done";
				case WebglAvatarState.FAILED:
					return "calculations failed, please try a different photo";
			}

			return "unknown state";
		}

		private void UpdateAvatarState(WebglAvatarState state, PipelineType pipelineType)
		{
			Debug.LogFormat("Pipeline: {0}, state: {1}", pipelineType, state);

			if (pipelineType == PipelineType.FACE)
			{
				// Don't display avatar status of animated face pipeline
				// It is being calculated in background
				return;
			}

			avatarState = state;
			statusText.text = string.Format("State: {0}", StatePretty(state));

			uploadButton.gameObject.SetActive(false);
			showButton.gameObject.SetActive(false);
			statusText.gameObject.SetActive(true);
			progressText.gameObject.SetActive(true);

			if (state == WebglAvatarState.FAILED)
				return;

			switch (state)
			{
				case WebglAvatarState.FINISHED:
					showButton.gameObject.SetActive(true);
					statusText.gameObject.SetActive(false);
					progressText.gameObject.SetActive(false);
					break;
				default:
					break;
			}
		}

		private IEnumerator Await(AsyncRequest r, PipelineType pipelineType)
		{
			while (!r.IsDone)
			{
				yield return null;

				if (r.IsError)
				{
					Debug.LogError(r.ErrorMessage);
					yield break;
				}

				if (pipelineType == PipelineType.HEAD)
				{
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

			if (pipelineType == PipelineType.HEAD)
				progressText.text = string.Empty;
		}

		private void ChangeControlsState(bool isEnabled)
		{
			controlsEnabled = isEnabled;
			browseButton.interactable = isEnabled;
			urlInput.interactable = isEnabled;
			uploadButton.interactable = isEnabled;
			showButton.interactable = isEnabled;
			urlButton.interactable = isEnabled;
		}

		private void ResetControlsToDefaultState()
		{
			urlUploadingStatusText.gameObject.SetActive(false);
			uploadButton.gameObject.SetActive(true);
			showButton.gameObject.SetActive(false);
			statusText.text = string.Empty;
			progressText.text = string.Empty;
		}

		private void ShowImageUploadingStatus(string message)
		{
			urlUploadingStatusText.text = message;
			urlUploadingStatusText.gameObject.SetActive(true);
		}

		private void HideImageUploadingStatus()
		{
			urlUploadingStatusText.gameObject.SetActive(false);
		}
	}
}
