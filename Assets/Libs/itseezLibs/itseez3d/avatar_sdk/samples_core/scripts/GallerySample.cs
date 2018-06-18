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
using System.IO;
using System.Linq;
using ItSeez3D.AvatarSdk.Core;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	/// <summary>
	/// Avatar states for a simple "state machine" implemented within GallerySample class.
	/// </summary>
	public enum GalleryAvatarState
	{
		UNKNOWN,
		GENERATING,
		COMPLETED,
		FAILED
	}

	/// <summary>
	/// This sample attempts to showcase the majority of the available API requests.
	/// </summary>
	public abstract class GallerySample : MonoBehaviour
	{
		// internal class stored avatar code and state
		protected class GalleryAvatar
		{
			public string code;
			public GalleryAvatarState state;
		}

		//Avatar provider - initialized once per application runtime.
		protected IAvatarProvider avatarProvider = null;

		// type of used SDK
		protected SdkType sdkType;

		// viewer implementation should be created in inherited classes
		protected IViewerImplementation viewerImplementation = null;

		#region UI

		// panel that contains gallery controls 
		public GameObject galleryControls;

		// used to edit avatar name/description in the cloud
		public GameObject editPanel;

		// array of avatar preview items in the gallery
		private Dictionary<string, AvatarPreview> avatarPreviews = new Dictionary<string, AvatarPreview>();

		// displayed status and progress of requests
		public Text progressText;

		// prefab that displays avatar preview in gallery
		public GameObject avatarPrefab;

		// panel that contains avatar previews
		public GameObject avatarsContainer;

		// label that displays current page number
		public Text currentPageText;

		// Test data, an array of jpeg-encoded sample selfies
		public TextAsset[] testPhotos;

		// scripts that allows to open image from the file system
		public FileBrowser fileBrowser = null;

		// button to upload photos from the file system
		public Button generateFromUserPhoto;

		// button to upload photos from the camera
		public Button generateFromCameraPhoto;

		#endregion

		#region State

		// list of all loaded avatar ids
		protected GalleryAvatar[] loadedAvatars = null;

		// Index of current gallery page, page indices start from 1.
		private int currentPage = 1;

		#endregion

		#region Lifecycle

		void Start()
		{
			StartCoroutine(Initialize());

			if (fileBrowser != null)
				fileBrowser.fileHandler = CreateNewAvatar;

#if UNITY_EDITOR || UNITY_WEBGL || UNITY_ANDROID || UNITY_IOS
			generateFromUserPhoto.gameObject.SetActive(true);
#endif
#if UNITY_ANDROID || UNITY_IOS
			generateFromCameraPhoto.gameObject.SetActive(true);
#endif
		}

#endregion

		#region Async utils

		/// <summary>
		/// Helper function that waits until async request finishes and keeps track of progress on request and it's
		/// subrequests. Note it does "yield return null" every time, which means that code inside the loop
		/// is executed on each frame, but after progress is updated the function does not block the main thread anymore.
		/// </summary>
		/// <param name="r">Async request to await.</param>
		/// <param name="avatarCode">If null the request does not correspond to the particular avatar, and the progress
		/// will be printed at the bottom of the screen below the "gallery". If not null then progress will
		/// be updated inside particular avatar preview item.</param>
		protected IEnumerator Await(AsyncRequest r, string avatarCode)
		{
			while (!r.IsDone)
			{
				yield return null;

				if (r.IsError)
				{
					Debug.LogError(r.ErrorMessage);
					yield break;
				}

				// Iterate over subrequests to obtain overall progress, as well as progress of the current stage.
				// E.g. main request: "Downloading avatar", overall progress 20%;
				// current stage: "Downloading mesh", progress 40%.
				// Level of nesting can be arbitrary, but generally less than three.
				var progress = new List<string>();
				AsyncRequest request = r;
				while (request != null)
				{
					progress.Add(string.Format("{0}: {1}%", request.State, request.ProgressPercent.ToString("0.0")));
					request = request.CurrentSubrequest;
				}

				if (string.IsNullOrEmpty(avatarCode))
				{
					// update progress at the top of the screen
					progressText.text = string.Join("  -->  ", progress.ToArray());
				}
				else
				{
					// update progress inside small gallery preview item
					UpdateAvatarProgress(avatarCode, string.Join("\n", progress.ToArray()));
				}
			}

			progressText.text = string.Empty;
		}

		#endregion

		#region "Custom" implementations of SDK interfaces

		private class CustomStringMgr : DefaultStringManager
		{
			// your implementation...
		}

		private class CustomPersistentStorage : DefaultPersistentStorage
		{
			/// <summary>
			/// Store haircuts downloaded from the server in a separate non-default folder.
			/// </summary>
			/// <returns>The haircuts directory.</returns>
			public override string GetHaircutsDirectory()
			{
				return EnsureDirectoryExists(Path.Combine(GetDataDirectory(), "haircuts_cloud"));
			}

			// your implementation...
		}

		#endregion

		#region Initialization

		private IEnumerator Initialize()
		{
			// First of all, initialize the SDK. This sample shows how to provide user-defined implementations for
			// the interfaces if needed. If you don't need to override the default behavior, just pass null instead.
			if (!AvatarSdkMgr.IsInitialized)
			{
				AvatarSdkMgr.Init(
					stringMgr: new CustomStringMgr(),
					storage: new CustomPersistentStorage()
				);
			}

			GameObject providerContainerGameObject = GameObject.Find("AvatarProviderContainer");
			if (providerContainerGameObject != null)
			{
				avatarProvider = providerContainerGameObject.GetComponent<AvatarProviderContainer>().avatarProvider;
			}
			else
			{
				// Initialization of the IAvatarProvider may take some time. 
				// We don't want to initialize it each time when the Gallery scene is loaded.
				// So we store IAvatarProvider instance in the object that will not destroyed during navigation between the scenes (Gallery -> ModelViewer -> Gallery).
				providerContainerGameObject = new GameObject("AvatarProviderContainer");
				DontDestroyOnLoad(providerContainerGameObject);
				AvatarProviderContainer providerContainer = providerContainerGameObject.AddComponent< AvatarProviderContainer>();
				avatarProvider = CoreTools.CreateAvatarProvider(sdkType);
				providerContainer.avatarProvider = avatarProvider;

				var initializeRequest = InitializeAvatarProviderAsync();
				yield return Await(initializeRequest, null);
				if (initializeRequest.IsError)
				{
					Debug.LogError("Avatar provider isn't initialized!");
					yield break;
				}
			}

			yield return UpdateAvatarList();

			// disable generation buttons until avatar provider initializes
			foreach (var button in galleryControls.GetComponentsInChildren<Button>(false))
				if (button.name.Contains("Generate"))
					button.interactable = true;
		}

		protected virtual AsyncRequest InitializeAvatarProviderAsync()
		{
			return avatarProvider.InitializeAsync();
		}

		#endregion

		#region Update information in the avatar preview

		private void UpdateAvatarState(string avatarCode, GalleryAvatarState state)
		{
			var avatar = GetAvatar(avatarCode);
			avatar.state = state;
			UpdateAvatarPreview(avatarCode, state);
		}

		private void UpdateAvatarPreview(string avatarCode, GalleryAvatarState state)
		{
			if (!avatarPreviews.ContainsKey(avatarCode))
				return;

			var preview = avatarPreviews[avatarCode];
			preview.UpdatePreview(avatarCode, state);
		}

		private void UpdateAvatarProgress(string avatarCode, string progressStr)
		{
			if (!avatarPreviews.ContainsKey(avatarCode))
				return;
			var preview = avatarPreviews[avatarCode];
			preview.UpdateProgress(progressStr);
		}

		#endregion

		#region Gallery page navigation

		/// <summary>
		/// Get a list of avatar that fit on the current page.
		/// </summary>
		/// <returns>List of avatars if the pageIdx is valid. Null if pageIdx is too high or too low.</returns>
		/// <param name="pageIdx">1-based index of the current page.</param>
		private string[] GetAvatarIdsForPage(int pageIdx)
		{
			if (loadedAvatars == null)
				return null;
			if (pageIdx < 1)
				return null;

			var panelW = avatarsContainer.GetComponent<RectTransform>().rect.width;
			var avatarW = avatarPrefab.GetComponent<RectTransform>().rect.width;
			var padding = 10;
			int numAvatarsPerPage = (int)(panelW / (avatarW + padding));
			int startIdx = (pageIdx - 1) * numAvatarsPerPage;

			var pageAvatars = new List<string>();
			for (int i = startIdx; i < loadedAvatars.Length && i < startIdx + numAvatarsPerPage; ++i)
				pageAvatars.Add(loadedAvatars[i].code);

			return pageAvatars.ToArray();
		}

		/// <summary>
		/// Display given list of avatars in the gallery (called when page is changed or when list is updated).
		/// </summary>
		private void UpdatePage(string[] pageAvatarIds)
		{
			// first - clean current previews, memory starts to leak if we don't do this
			foreach (var child in avatarsContainer.GetComponentsInChildren<AvatarPreview>())
			{
				child.CleanUp();
				Destroy(child.gameObject);
			}
			avatarPreviews.Clear();

			for (int i = 0; i < pageAvatarIds.Length; ++i)
			{
				var avatarPreview = GameObject.Instantiate(avatarPrefab);
				avatarPreview.transform.localScale = avatarsContainer.transform.lossyScale;
				avatarPreview.transform.SetParent(avatarsContainer.transform);
				var preview = avatarPreview.GetComponent<AvatarPreview>();
				var avatarCode = pageAvatarIds[i];
				avatarPreviews[avatarCode] = preview;

				var avatar = GetAvatar(avatarCode);
				UpdateAvatarState(avatarCode, avatar.state);
				InitAvatarPreview(preview, pageAvatarIds[i], avatar.state);
			}
		}

		/// <summary>
		/// Initialize avatar preview that is displayed in gallery.
		/// Avatar preview slightly different for Cloud and Offline SDks
		/// </summary>
		protected virtual void InitAvatarPreview(AvatarPreview preview, string avatarCode, GalleryAvatarState avatarState)
		{
			preview.InitPreview(this, avatarCode, avatarState, false);
		}

		private void ShowPage(int newPage)
		{
			var avatarsForPage = GetAvatarIdsForPage(newPage);
			if (avatarsForPage == null)
				return;

			if (avatarsForPage.Length == 0 && newPage > currentPage)
			{
				Debug.LogFormat("Next page is empty, ignore...");
				return;
			}

			UpdatePage(avatarsForPage);
			currentPage = newPage;
			currentPageText.text = currentPage.ToString();
		}

		public void OnPrevPage()
		{
			ShowPage(currentPage - 1);
		}

		public void OnNextPage()
		{
			ShowPage(currentPage + 1);
		}

		#endregion

		#region Avatar creation and processing

		/// <summary>
		/// Detects created avatars and displays them in the gallery.
		/// </summary>
		protected IEnumerator UpdateAvatarList()
		{
			Debug.LogFormat("Updating avatar list...");

			// For this sample we basically get all avatars created by the current player (but no more than a 1000,
			// just in case). Then pagination is done locally.
			// This should be all right for almost all practical situations. However if this is not suitable for your app
			// you can implement custom pagination logic using the low-level Connection API.
			const int maxAvatars = 1000;
			var avatarsRequest = GetAllAvatarsAsync(maxAvatars);
			yield return Await(avatarsRequest, null);
			if (avatarsRequest.IsError)
				yield break;

			loadedAvatars = avatarsRequest.Result;

			// If some avatars were deleted on the server we might need to return to the previous page in case the
			// current page is empty.
			while (currentPage > 1)
			{
				var pageAvatars = GetAvatarIdsForPage(currentPage);
				if (pageAvatars == null || pageAvatars.Length == 0)
				{
					--currentPage;
					continue;
				}
				else
					break;
			}

			// display current page using updated list of avatars
			ShowPage(currentPage);
		}

		/// <summary>
		/// Create avatar and save photo to disk.
		/// </summary>
		private IEnumerator CreateNewAvatar(byte[] photoBytes)
		{
			var initializeAvatar = avatarProvider.InitializeAvatarAsync(photoBytes);
			yield return Await(initializeAvatar, null);

			string avatarCode = initializeAvatar.Result;
			if (initializeAvatar.IsError)
			{
				UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
				yield break;
			}

			yield return UpdateAvatarList();
			UpdateAvatarState(avatarCode, GalleryAvatarState.GENERATING);

			var calculateAvatar = avatarProvider.StartAndAwaitAvatarCalculationAsync(avatarCode);
			yield return Await(calculateAvatar, avatarCode);
			if (calculateAvatar.IsError)
			{
				UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
				yield break;
			}

			var downloadAvatar = avatarProvider.MoveAvatarModelToLocalStorageAsync(avatarCode, true, true);
			yield return Await(downloadAvatar, avatarCode);
			if (downloadAvatar.IsError)
			{
				UpdateAvatarState(avatarCode, GalleryAvatarState.FAILED);
				yield break;
			}

			UpdateAvatarState(avatarCode, GalleryAvatarState.COMPLETED);
		}

		/// <summary>
		/// Button click handler.
		/// </summary>
		public void OnGenerateFromRandomPhoto()
		{
			var testPhotoIdx = UnityEngine.Random.Range(0, testPhotos.Length);
			var testPhoto = testPhotos[testPhotoIdx];
			StartCoroutine(CreateNewAvatar(testPhoto.bytes));
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
			yield return CreateNewAvatar(bytes);
		}

		/// <summary>
		/// Button click handler.
		/// </summary>
		public void OnShowAvatar(string avatarCode)
		{
			var avatar = GetAvatar(avatarCode);
			if (avatar.state != GalleryAvatarState.COMPLETED)
			{
				Debug.LogErrorFormat("Avatar not ready to be opened: {0}, state: {1}", avatarCode, avatar.state);
				return;
			}

			AvatarViewer.SetSceneParams(new AvatarViewer.SceneParams()
			{
				avatarCode = avatarCode,
				sceneToReturn = SceneManager.GetActiveScene().name,
				viewerImplementation = viewerImplementation,
				avatarProvider = this.avatarProvider
			});
			SceneManager.LoadScene(Scenes.GetSceneName(SceneType.AVATAR_VIEWER));
		}

		/// <summary>
		/// Finds the avatar with the given code in the loadedAvatars
		/// </summary>
		private GalleryAvatar GetAvatar(string avatarCode)
		{
			return loadedAvatars.FirstOrDefault(a => string.Compare(a.code, avatarCode) == 0);
		}

		#endregion

		#region Edit and delete avatar

		public virtual void OnEditAvatar(string avatarCode)
		{
		}


		public virtual void OnEditConfirm()
		{
		}

		/// <summary>
		/// Delete local avatar files and request server to delete all data permanently. Can't undo this.
		/// </summary>
		private IEnumerator DeleteAvatar(string avatarCode)
		{
			AvatarPreview preview = avatarPreviews[avatarCode];
			avatarPreviews.Remove(avatarCode);
			preview.CleanUp();
			Destroy(preview.gameObject);

			var deleteRequest = avatarProvider.DeleteAvatarAsync(avatarCode);
			yield return deleteRequest;
			yield return UpdateAvatarList();
		}

		public void OnDeleteAvatar(string avatarCode)
		{
			StartCoroutine(DeleteAvatar(avatarCode));
		}

		#endregion

		#region abstract methods
		protected abstract AsyncRequest<GalleryAvatar[]> GetAllAvatarsAsync(int maxItems);
		#endregion
	}
}
