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
using System.Runtime.InteropServices;
using System.Security.Cryptography;
using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;
using ItSeez3D.AvatarSdkSamples.Core;
using ItSeez3D.AvatarSdkSamples.Cloud;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class AvatarViewer : MonoBehaviour
	{
		public class SceneParams
		{
			public string avatarCode;
			public string sceneToReturn;
			public bool withPlane = true;
			public IViewerImplementation viewerImplementation;
			public IAvatarProvider avatarProvider;
		};

		#region Scene and project dependencies

		// Material uses unlit shader
		public Material unlitMaterial;

		// Material uses lit shader
		public Material litMaterial;

		// Unlit material for haircuts
		public Material haircutUnlitMaterial;

		// Lit material for haircuts
		public Material haircutLitMaterial;

		// Camera for the scene
		public Camera sceneCamera;

		// Parent object of the camera
		public GameObject cameraPosition;

		// Parent object of the camera when it should be moved further
		public GameObject farCameraPosition;

		// The shadows are casted onto this plane.
		public GameObject bottomPlane;

		#endregion

		#region UI

		public GameObject avatarControls;
		public Text progressText;
		public Image photoPreview;
		public Button convertToObjButton;
		public Button fbxExportButton;
		public Button prefabButton;

		#endregion

		#region private memebers

		// Parameters needed to initialize scene and show avatar. Should be set before showing the viewer scene
		private static SceneParams initParams = null;

		// Current displayed avatar
		private string currentAvatarCode;

		// Scene that will be shown after clicking on the back button
		private string sceneToReturn;

		// Whether to show the plane underneath the head or not (to test shadows).
		private bool showPlane;

		// This GameObject represents head in the scene.
		private GameObject headObject = null;

		// Array of haircut names
		private string[] avatarHaircuts = null;

		// Haircut index of the current avatar, zero for bald head.
		private int currentHaircut = 0;

		// Positions of the instantiated objects relative to base model.
		private List<Matrix4x4> deltaMatrixList = new List<Matrix4x4> ();

		// Specific implementation of some methods
		private IViewerImplementation viewerImplementation;

		// AvatarProvider to retrieve head mesh and texture
		private IAvatarProvider avatarProvider;

		#endregion

		#region Constants

		private const string BALD_HAIRCUT_NAME = "bald";
		private const string HEAD_OBJECT_NAME = "ItSeez3D Head";
		private const string HAIRCUT_OBJECT_NAME = "ItSeez3D Haircut";
		private const string AVATAR_OBJECT_NAME = "ItSeez3D Avatar";
		private readonly Vector3 avatarScale = new Vector3 (10, 10, 10);

		#endregion

		#region Events

		public delegate void DisplayedHaircutChanged (string newHaircutId);

		public event DisplayedHaircutChanged displayedHaircutChanged;

		public delegate void InstantiatingChanged (bool withInstantiating);

		public event InstantiatingChanged instantiatingChanged;

		public delegate void ShaderTypeChanged (bool isUnlitShader);

		public event ShaderTypeChanged shaderTypeChanged;

		#endregion

		#region Methods to call event handlers

		private void OnDisplayedHaircutChanged (string newHaircutId)
		{
			if (displayedHaircutChanged != null)
				displayedHaircutChanged (newHaircutId);
		}

		private void OnInstantiatingChanged (bool withInstantiating)
		{
			if (instantiatingChanged != null)
				instantiatingChanged (withInstantiating);
		}

		private void OnShaderTypeChanged (bool isUnlitShader)
		{
			if (shaderTypeChanged != null)
				shaderTypeChanged (isUnlitShader);
		}

		#endregion

		#region static methods

		public static void SetSceneParams (SceneParams sceneParams)
		{
			initParams = sceneParams;
		}

		#endregion

		#region properties

		// Flag indicates if the unlit shader is used for head.
		public bool IsUnlitMode {
			get;
			private set;
		}

		// Flag indicates if we need to render several models by using GPU instantiating.
		public bool IsInstantiatingMode {
			get;
			private set;
		}

		public GameObject HaircutObject {
			get {
				var haircutObj = GameObject.Find (HAIRCUT_OBJECT_NAME);
				return haircutObj;
			}
		}

		#endregion

		#region Lifecycle

		void Start ()
		{
			// default values for properties
			IsUnlitMode = true;
			IsInstantiatingMode = false;

			// required for transparent hair shader
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_EDITOR
			QualitySettings.antiAliasing = 8;
#else
			QualitySettings.antiAliasing = 4;
#endif

			// initialize positions of the instantiated models
			var d = 4.2f;
			for (int i = -3; i <= 3; i++)
				for (int j = -3; j <= 3; j++) {
					if (i == 0 && j == 0)
						continue;
					var matrix = Matrix4x4.identity;
					matrix.m03 += i * d;
					matrix.m23 += j * d;
					deltaMatrixList.Add (matrix);
				}

			StartCoroutine (InitializeScene ());
		}

		/// <summary>
		/// Called by Unity on every frame.
		/// </summary>
		void Update ()
		{
			// example on how to cheaply render multiple instances of 3D avatar
			if (IsInstantiatingMode && headObject != null) {
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer> ();
				Mesh headMesh = headMeshRenderer.sharedMesh;
				Material headMaterial = headMeshRenderer.material;
				Mesh haircutMesh = null;
				Material haircutMaterial = null;
				var haircutObj = GameObject.Find (HAIRCUT_OBJECT_NAME);
				if (haircutObj != null) {
					haircutMesh = haircutObj.GetComponent<MeshFilter> ().mesh;
					haircutMaterial = haircutObj.GetComponent<MeshRenderer> ().material;
				}

				foreach (Matrix4x4 deltaMatrix in deltaMatrixList) {
					Matrix4x4 headMatrix = headObject.transform.localToWorldMatrix;
					headMatrix.m03 += deltaMatrix.m03;
					headMatrix.m23 += deltaMatrix.m23;
					Graphics.DrawMesh (headMesh, headMatrix, headMaterial, 0);

					if (haircutMesh != null) {
						Matrix4x4 haircutMatrix = haircutObj.transform.localToWorldMatrix;
						haircutMatrix.m03 += deltaMatrix.m03;
						haircutMatrix.m23 += deltaMatrix.m23;
						Graphics.DrawMesh (haircutMesh, haircutMatrix, haircutMaterial, 0);
					}
				}
			}
		}

		#endregion

		#region UI controls events handling

		/// <summary>
		/// Button click handler. Go back to the gallery.
		/// </summary>
		public virtual void OnBack ()
		{
			SceneManager.LoadScene (sceneToReturn);
		}

		public void OnPrevHaircut ()
		{
			StartCoroutine (ChangeHaircut (currentHaircut - 1));
		}

		public void OnNextHaircut ()
		{
			StartCoroutine (ChangeHaircut (currentHaircut + 1));
		}

		public void OnShaderCheckboxChanged (bool isChecked)
		{
			IsUnlitMode = isChecked;
			var headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer> ();
			headMeshRenderer.material.shader = isChecked ? unlitMaterial.shader : litMaterial.shader;

			var haircutObj = GameObject.Find (HAIRCUT_OBJECT_NAME);
			if (haircutObj != null) {
				MeshRenderer haircutMeshRenderer = haircutObj.GetComponent<MeshRenderer> ();
				haircutMeshRenderer.material.shader = IsUnlitMode ? haircutUnlitMaterial.shader : haircutLitMaterial.shader;
			}

			OnShaderTypeChanged (IsUnlitMode);
		}

		public void OnInstantiateCheckboxChanged (bool isChecked)
		{
			IsInstantiatingMode = isChecked;
			GetComponent<AnimationManager> ().animationsPanel.SetActive (!IsInstantiatingMode);

			if (isChecked)
				sceneCamera.transform.SetParent (farCameraPosition.transform);
			else
				sceneCamera.transform.SetParent (cameraPosition.transform);
			sceneCamera.transform.localPosition = Vector3.zero;
			sceneCamera.transform.localRotation = Quaternion.identity;

			OnInstantiatingChanged (IsInstantiatingMode);
		}

		public void ConvertAvatarToObjFormat ()
		{
			var haircutRecoloring = GetComponent<HaircutRecoloring> ();
			string haircutName = avatarHaircuts[currentHaircut];
			if (string.Compare(haircutName, BALD_HAIRCUT_NAME) == 0)
				haircutName = string.Empty;
			viewerImplementation.ConvertAvatarToObjFormat (currentAvatarCode, haircutName, haircutRecoloring.CurrentColor, haircutRecoloring.CurrentTint);
		}

		public void ExportAvatarAsFbx ()
		{
			var haircutRecoloring = GetComponent<HaircutRecoloring> ();
			string haircutName = avatarHaircuts[currentHaircut];
			if (string.Compare(haircutName, BALD_HAIRCUT_NAME) == 0)
				haircutName = string.Empty;
			viewerImplementation.ExportAvatarAsFBX (currentAvatarCode, haircutName, haircutRecoloring.CurrentColor, haircutRecoloring.CurrentTint);
		}

		public void CreateAvatarPrefab ()
		{
#if UNITY_EDITOR
			AvatarPrefabBuilder.CreateAvatarPrefab (GameObject.Find (AVATAR_OBJECT_NAME), HEAD_OBJECT_NAME, HAIRCUT_OBJECT_NAME, currentAvatarCode);
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
		IEnumerator Await (AsyncRequest r)
		{
			while (!r.IsDone) {
				yield return null;

				if (r.IsError) {
					Debug.LogError (r.ErrorMessage);
					yield break;
				}

				progressText.text = string.Format("{0}: {1}%", r.State, r.ProgressPercent.ToString("0.0"));
			}

			progressText.text = string.Empty;
		}

		#endregion

		#region Initialization routine

		private IEnumerator InitializeScene ()
		{
			if (initParams != null) {
				avatarProvider = initParams.avatarProvider;
				viewerImplementation = initParams.viewerImplementation;
				sceneToReturn = initParams.sceneToReturn;
				currentAvatarCode = initParams.avatarCode;
				showPlane = initParams.withPlane;
				initParams = null;

				if (viewerImplementation.IsObjConvertEnabled)
					convertToObjButton.gameObject.SetActive (true);

				if (viewerImplementation.IsFBXExportEnabled)
					fbxExportButton.gameObject.SetActive (true);

#if UNITY_EDITOR
				prefabButton.gameObject.SetActive (true);
#endif

				yield return ShowAvatar (currentAvatarCode);
			} else
				Debug.LogError ("Scene parameters were no set!");
		}

		#endregion

		#region Avatar processing

		/// <summary>
		/// Show avatar in the scene. Also load haircut information to allow further haircut change.
		/// </summary>
		private IEnumerator ShowAvatar (string avatarCode)
		{
			StartCoroutine (SampleUtils.DisplayPhotoPreview (avatarCode, photoPreview));

			progressText.text = string.Empty;
			currentHaircut = 0;

			var avatarObject = new GameObject (AVATAR_OBJECT_NAME);

			var headMeshRequest = avatarProvider.GetHeadMeshAsync(avatarCode, true);
			yield return Await(headMeshRequest);

			if (headMeshRequest.IsError)
			{
				Debug.LogError("Could not load avatar from disk!");
			}
			else
			{
				TexturedMesh texturedMesh = headMeshRequest.Result;

				// game object can be deleted if we opened another avatar
				if (avatarObject != null && avatarObject.activeSelf)
				{
					avatarObject.AddComponent<RotateByMouse>();

					headObject = new GameObject(HEAD_OBJECT_NAME);
					var meshRenderer = headObject.AddComponent<SkinnedMeshRenderer>();
					meshRenderer.sharedMesh = texturedMesh.mesh;
					meshRenderer.material = IsUnlitMode ? unlitMaterial : litMaterial;
					meshRenderer.material.mainTexture = texturedMesh.texture;
					headObject.transform.SetParent(avatarObject.transform);
					avatarObject.transform.localScale = avatarScale;

					GetComponent<AnimationManager>().CreateAnimator(headObject);
				}
			}

			if (showPlane)
				bottomPlane.SetActive (true);

			var haircutsRequest = avatarProvider.GetHaircutsIdAsync(avatarCode);
			yield return Await(haircutsRequest);
			
			//Add fake "bald" haircut
			var haircutsList = haircutsRequest.Result.ToList();
			haircutsList.Insert(0, BALD_HAIRCUT_NAME);
			avatarHaircuts = haircutsList.ToArray();

			avatarControls.SetActive (true);
			OnDisplayedHaircutChanged (avatarHaircuts[currentHaircut]);
		}

		#endregion

		#region Haircut handling

		/// <summary>
		/// Change the displayed haircut. Make controls inactive while haircut is being loaded to prevent
		/// multiple coroutines running at once.
		/// </summary>
		/// <param name="newIdx">New haircut index.</param>
		private IEnumerator ChangeHaircut (int newIdx)
		{
			ChangeButtonsInteractability (false);

			var previousIdx = currentHaircut;
			yield return StartCoroutine (ChangeHaircutFunc (newIdx));
			if (previousIdx != currentHaircut)
				OnDisplayedHaircutChanged (avatarHaircuts[currentHaircut]);

			ChangeButtonsInteractability (true);
		}

		/// <summary>
		/// Actually load the haircut model and texture and display it in the scene (aligned with the head).
		/// </summary>
		/// <param name="newIdx">Index of the haircut.</param>
		private IEnumerator ChangeHaircutFunc (int newIdx)
		{
			if (newIdx < 0 || newIdx >= avatarHaircuts.Length)
				yield break;

			currentHaircut = newIdx;
			string haircutName = avatarHaircuts[currentHaircut];
			var haircutObj = GameObject.Find (HAIRCUT_OBJECT_NAME);

			// bald head is just absence of haircut
			if (string.Compare(haircutName, BALD_HAIRCUT_NAME) == 0) {
				Destroy (haircutObj);
				yield break;
			}

			var haircurtMeshRequest = avatarProvider.GetHaircutMeshAsync(currentAvatarCode, haircutName);
			yield return Await(haircurtMeshRequest);
			if (haircurtMeshRequest.IsError)
				yield break;

			Destroy (haircutObj);

			var texturedMesh = haircurtMeshRequest.Result;
			var meshObject = new GameObject (HAIRCUT_OBJECT_NAME);
			meshObject.AddComponent<MeshFilter> ().mesh = texturedMesh.mesh;
			var meshRenderer = meshObject.AddComponent<MeshRenderer> ();

			meshRenderer.material = IsUnlitMode ? haircutUnlitMaterial : haircutLitMaterial;
			meshRenderer.material.mainTexture = texturedMesh.texture;

			// One male haircut looks better with disabled culling
			if (haircutName.Contains ("makehuman"))
				meshRenderer.material.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Off);
			else
				meshRenderer.material.SetInt ("_Cull", (int)UnityEngine.Rendering.CullMode.Back);

			// ensure that haircut is rotated just like the head
			var avatarObject = GameObject.Find (AVATAR_OBJECT_NAME);
			if (avatarObject != null) {
				meshObject.transform.SetParent (avatarObject.transform);
				meshObject.transform.localRotation = Quaternion.identity;
				meshObject.transform.localScale = Vector3.one;
			}

			yield return null;  // only after the next frame the textures and materials are actually updated in the scene
		}

		#endregion

		private void ChangeButtonsInteractability (bool isEnabled)
		{
			foreach (var button in avatarControls.GetComponentsInChildren<Button>())
				button.interactable = isEnabled;

			foreach (var toggle in avatarControls.GetComponentsInChildren<Toggle>())
				toggle.interactable = isEnabled;
		}
	}
}
