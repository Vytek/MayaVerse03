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
using ItSeez3D.AvatarSdkSamples.Cloud;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

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
			public IAvatarProvider avatarProvider;
			public bool showSettings = true;
			public bool useAnimations = true;

			// Is used for WebGL Demo sample to display two avatars (head and face)
			public AsyncRequest<AvatarData> faceAvatarRequest = null;
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
		public GameObject settingsPanel;
		public GameObject pipelinesPanel;
		public Text haircutText;
		public Text blendshapeText;
		public GameObject haircutsPanel;
		public GameObject animationsPanel;
		public GameObject blendshapesPanel;
		public ItemsSelectingView blendshapesSelectingView;
		public HaircutsSelectingView haircutsSelectingView;
		public Toggle faceToggle;

		#endregion

		#region private memebers

		// Parameters needed to initialize scene and show avatar. Should be set before showing the viewer scene
		private static SceneParams initParams = null;

		// Current displayed avatar
		private string currentAvatarCode;

		// Scene that will be shown after clicking on the back button
		private string sceneToReturn;

		// This GameObject represents head in the scene.
		private GameObject headObject = null;

		// Array of haircut names
		private string[] avatarHaircuts = null;

		// Haircut index of the current avatar, zero for bald head.
		private int currentHaircut = 0;

		// Positions of the instantiated objects relative to base model.
		private List<Matrix4x4> deltaMatrixList = new List<Matrix4x4>();

		// AvatarProvider to retrieve head mesh and texture
		private IAvatarProvider avatarProvider;

		// True is animations will be used, in other case single blendshapes will be used
		private bool useAnimations = true;

		// Blendshapes names with their index in avatar mesh
		private Dictionary<int, string> availableBlendshapes = new Dictionary<int, string>();

		// Blendshape index of the current avatar
		private int currentBlendshape = 0;

		// Code of the head avatar for WebGL demo
		private string headAvatarCode;

		// Code of the faca avatar for WebGL demo
		private string faceAvatarCode;

		// Cached haircuts for avatars
		private Dictionary<string, string[]> cachedHaircuts = new Dictionary<string, string[]>();

		// names of the animations to play
		private List<string> animations = new List<string> {
			"smile",
			"blink",
			"kiss",
			"puff",
			"yawning",
			"chewing",
			"mouth_left_right",
		};

		// flag indicates if this is model from the WebGL demo
		private bool isWebGLDemo = false; 

		#endregion

		#region Constants

		private const string BALD_HAIRCUT_NAME = "bald";
		private const string HEAD_OBJECT_NAME = "ItSeez3D Head";
		private const string HAIRCUT_OBJECT_NAME = "ItSeez3D Haircut";
		private const string AVATAR_OBJECT_NAME = "ItSeez3D Avatar";
		private Vector3 faceAvatarScale = new Vector3(10, 10, 10);
		private Vector3 headAvatarScale = new Vector3(8, 8, 8);

		#endregion

		#region Events

		public delegate void DisplayedHaircutChanged(string newHaircutId);

		public event DisplayedHaircutChanged displayedHaircutChanged;

		public delegate void InstantiatingChanged(bool withInstantiating);

		public event InstantiatingChanged instantiatingChanged;

		public delegate void ShaderTypeChanged(bool isUnlitShader);

		public event ShaderTypeChanged shaderTypeChanged;

		#endregion

		#region Methods to call event handlers

		private void OnDisplayedHaircutChanged(string newHaircutId)
		{
			int slashPos = newHaircutId.LastIndexOfAny(new char[] { '\\', '/' });
			haircutText.text = slashPos == -1 ? newHaircutId : newHaircutId.Substring(slashPos + 1);
			if (displayedHaircutChanged != null)
				displayedHaircutChanged(newHaircutId);
		}

		private void OnInstantiatingChanged(bool withInstantiating)
		{
			if (instantiatingChanged != null)
				instantiatingChanged(withInstantiating);
		}

		private void OnShaderTypeChanged(bool isUnlitShader)
		{
			if (shaderTypeChanged != null)
				shaderTypeChanged(isUnlitShader);
		}

		#endregion

		#region static methods

		public static void SetSceneParams(SceneParams sceneParams)
		{
			initParams = sceneParams;
		}

		#endregion

		#region properties

		// Flag indicates if the unlit shader is used for head.
		public bool IsUnlitMode
		{
			get;
			private set;
		}

		// Flag indicates if we need to render several models by using GPU instantiating.
		public bool IsInstantiatingMode
		{
			get;
			private set;
		}

		public GameObject HaircutObject
		{
			get
			{
				var haircutObj = GameObject.Find(HAIRCUT_OBJECT_NAME);
				return haircutObj;
			}
		}

		#endregion

		#region Lifecycle

		void Start()
		{
			avatarControls.SetActive(false);

			// default values for properties
			IsUnlitMode = true;
			IsInstantiatingMode = false;

			// required for transparent hair shader
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN || UNITY_EDITOR
			QualitySettings.antiAliasing = 8;
#elif !UNITY_WEBGL
			QualitySettings.antiAliasing = 4;
#endif

			// initialize positions of the instantiated models
			var d = 4.2f;
			for (int i = -3; i <= 3; i++)
				for (int j = -3; j <= 3; j++)
				{
					if (i == 0 && j == 0)
						continue;
					var matrix = Matrix4x4.identity;
					matrix.m03 += i * d;
					matrix.m23 += j * d;
					deltaMatrixList.Add(matrix);
				}

			StartCoroutine(InitializeScene());
		}

		/// <summary>
		/// Called by Unity on every frame.
		/// </summary>
		void Update()
		{
			// example on how to cheaply render multiple instances of 3D avatar
			if (IsInstantiatingMode && headObject != null)
			{
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer>();
				Mesh headMesh = headMeshRenderer.sharedMesh;
				Material headMaterial = headMeshRenderer.material;
				Mesh haircutMesh = null;
				Material haircutMaterial = null;
				var haircutObj = GameObject.Find(HAIRCUT_OBJECT_NAME);
				if (haircutObj != null)
				{
					haircutMesh = haircutObj.GetComponent<MeshFilter>().mesh;
					haircutMaterial = haircutObj.GetComponent<MeshRenderer>().material;
				}

				foreach (Matrix4x4 deltaMatrix in deltaMatrixList)
				{
					Matrix4x4 headMatrix = headObject.transform.localToWorldMatrix;
					headMatrix.m03 += deltaMatrix.m03;
					headMatrix.m23 += deltaMatrix.m23;
					Graphics.DrawMesh(headMesh, headMatrix, headMaterial, 0);

					if (haircutMesh != null)
					{
						Matrix4x4 haircutMatrix = haircutObj.transform.localToWorldMatrix;
						haircutMatrix.m03 += deltaMatrix.m03;
						haircutMatrix.m23 += deltaMatrix.m23;
						Graphics.DrawMesh(haircutMesh, haircutMatrix, haircutMaterial, 0);
					}
				}
			}
		}

		#endregion

		#region UI controls events handling

		/// <summary>
		/// Button click handler. Go back to the gallery.
		/// </summary>
		public virtual void OnBack()
		{
			SceneManager.LoadScene(sceneToReturn);
		}

		public void OnPrevHaircut()
		{
			StartCoroutine(ChangeHaircut(currentHaircut - 1));
		}

		public void OnNextHaircut()
		{
			StartCoroutine(ChangeHaircut(currentHaircut + 1));
		}

		public void OnPrevBlendshape()
		{
			ChangeCurrentBlendshape(currentBlendshape - 1);
		}

		public void OnNextBlendshape()
		{
			ChangeCurrentBlendshape(currentBlendshape + 1);
		}

		public void OnBlendshapeListButtonClick()
		{
			avatarControls.SetActive(false);
			blendshapesSelectingView.Show(new List<string>() { availableBlendshapes[currentBlendshape] }, list =>
			{
				avatarControls.SetActive(true);
				// Find KeyValuePair for selected blendshape name. Assume that returned list contains only one element.
				var pair = availableBlendshapes.FirstOrDefault(p => p.Value == list[0]);
				ChangeCurrentBlendshape(pair.Key);
			});
		}

		public void OnHaircutListButtonClick()
		{
			avatarControls.SetActive(false);
			haircutsSelectingView.Show(new List<string>() { avatarHaircuts[currentHaircut] }, list =>
			{
				avatarControls.SetActive(true);
				// Find index of the selected haircut.
				for (int i = 0; i < avatarHaircuts.Length; i++)
				{
					if (avatarHaircuts[i] == list[0])
					{
						StartCoroutine(ChangeHaircut(i));
						break;
					}
				}
			});
		}

		public void OnShaderCheckboxChanged(bool isChecked)
		{
			IsUnlitMode = isChecked;
			var headMeshRenderer = headObject.GetComponent<SkinnedMeshRenderer>();
			headMeshRenderer.material.shader = isChecked ? unlitMaterial.shader : litMaterial.shader;

			var haircutObj = GameObject.Find(HAIRCUT_OBJECT_NAME);
			if (haircutObj != null)
			{
				MeshRenderer haircutMeshRenderer = haircutObj.GetComponent<MeshRenderer>();
				haircutMeshRenderer.material.shader = IsUnlitMode ? haircutUnlitMaterial.shader : haircutLitMaterial.shader;
			}

			OnShaderTypeChanged(IsUnlitMode);
		}

		public void OnInstantiateCheckboxChanged(bool isChecked)
		{
			IsInstantiatingMode = isChecked;
			GetComponent<AnimationManager>().animationsPanel.SetActive(!IsInstantiatingMode);

			if (isChecked)
				sceneCamera.transform.SetParent(farCameraPosition.transform);
			else
				sceneCamera.transform.SetParent(cameraPosition.transform);
			sceneCamera.transform.localPosition = Vector3.zero;
			sceneCamera.transform.localRotation = Quaternion.identity;

			OnInstantiatingChanged(IsInstantiatingMode);
		}

		public void OnPipelineCheckboxChanged(bool isChecked)
		{
			if (isChecked)
			{
				currentAvatarCode = faceToggle.isOn ? faceAvatarCode : headAvatarCode;
				StartCoroutine(ShowAvatar(currentAvatarCode));
			}
		}

		public void ConvertAvatarToObjFormat()
		{
			var haircutRecoloring = GetComponent<HaircutRecoloring>();
			string haircutName = string.Empty;
			if (avatarHaircuts != null && string.Compare(avatarHaircuts[currentHaircut], BALD_HAIRCUT_NAME) != 0)
				haircutName = avatarHaircuts[currentHaircut];

			var outputObjDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(currentAvatarCode, AvatarSubdirectory.OBJ_EXPORT);
			var outputObjFile = Utils.CombinePaths(outputObjDir, "model.obj");
			CoreTools.AvatarPlyToObj(currentAvatarCode, AvatarFile.MESH_PLY, AvatarFile.TEXTURE, outputObjFile);

			if (!string.IsNullOrEmpty(haircutName))
			{
				var haircutObjFile = Path.Combine(Path.GetDirectoryName(outputObjFile), HaircutIdToFileName(haircutName, "obj"));
				CoreTools.HaircutPlyToObj(currentAvatarCode, haircutName, haircutObjFile, haircutRecoloring.CurrentColor, haircutRecoloring.CurrentTint);
			}

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			System.Diagnostics.Process.Start(outputObjDir);
#else
			progressText.text = string.Format("OBJ file was saved to avatar directory");
#endif
		}

		public void ExportAvatarAsFbx()
		{
			var haircutRecoloring = GetComponent<HaircutRecoloring>();
			string haircutName = string.Empty;
			if (avatarHaircuts != null && string.Compare(avatarHaircuts[currentHaircut], BALD_HAIRCUT_NAME) != 0)
				haircutName = avatarHaircuts[currentHaircut];

			var exportDir = AvatarSdkMgr.Storage().GetAvatarSubdirectory(currentAvatarCode, AvatarSubdirectory.FBX_EXPORT);
			var outputFbxFile = Utils.CombinePaths(exportDir, "model.fbx");
			CoreTools.ExportAvatarAsFbx(currentAvatarCode, outputFbxFile);

			if (!string.IsNullOrEmpty(haircutName))
			{
				var haircutFbxFile = Path.Combine(Path.GetDirectoryName(outputFbxFile), HaircutIdToFileName(haircutName, "fbx"));
				CoreTools.HaircutPlyToFbx(currentAvatarCode, haircutName, haircutFbxFile, haircutRecoloring.CurrentColor, haircutRecoloring.CurrentTint);
			}

#if UNITY_EDITOR || UNITY_STANDALONE_WIN
			System.Diagnostics.Process.Start(exportDir);
#else
			progressText.text = string.Format("FBX file was saved to avatar directory");
#endif
		}

		public void CreateAvatarPrefab()
		{
#if UNITY_EDITOR
			AvatarPrefabBuilder.CreateAvatarPrefab(GameObject.Find(AVATAR_OBJECT_NAME), HEAD_OBJECT_NAME, HAIRCUT_OBJECT_NAME, currentAvatarCode);
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
		IEnumerator Await(AsyncRequest r)
		{
			while (!r.IsDone)
			{
				yield return null;

				if (r.IsError)
				{
					Debug.LogError(r.ErrorMessage);
					yield break;
				}

				progressText.text = string.Format("{0}: {1}%", r.State, r.ProgressPercent.ToString("0.0"));
			}

			progressText.text = string.Empty;
		}

		#endregion

		#region Initialization routine

		private IEnumerator InitializeScene()
		{
			if (initParams != null)
			{
				avatarProvider = initParams.avatarProvider;
				sceneToReturn = initParams.sceneToReturn;
				currentAvatarCode = initParams.avatarCode;
				useAnimations = initParams.useAnimations;

#if !UNITY_WEBGL
				IMeshConverter meshConverter = AvatarSdkMgr.IoCContainer.Create<IMeshConverter>();
				if (meshConverter.IsObjConvertEnabled)
					convertToObjButton.gameObject.SetActive(true);

				if (meshConverter.IsFBXExportEnabled)
					fbxExportButton.gameObject.SetActive(true);
#endif

#if UNITY_EDITOR
				prefabButton.gameObject.SetActive(true);
#endif
				settingsPanel.SetActive(initParams.showSettings);
				animationsPanel.SetActive(initParams.useAnimations);
				blendshapesPanel.SetActive(!initParams.useAnimations);
				if (initParams.faceAvatarRequest != null)
				{
					isWebGLDemo = true;
					headAvatarCode = currentAvatarCode;
					StartCoroutine(WaitFaceAvatarCalculations(initParams.faceAvatarRequest));
				}
				initParams = null;

				yield return ShowAvatar(currentAvatarCode);
			}
			else
				Debug.LogError("Scene parameters were no set!");
		}

		#endregion

		#region Avatar processing

		/// <summary>
		/// Show avatar in the scene. Also load haircut information to allow further haircut change.
		/// </summary>
		private IEnumerator ShowAvatar(string avatarCode)
		{
			ChangeControlsInteractability(false);
			yield return new WaitForSeconds(0.05f);

			StartCoroutine(SampleUtils.DisplayPhotoPreview(avatarCode, photoPreview));

			progressText.text = string.Empty;
			currentHaircut = 0;

			var currentAvatar = GameObject.Find(AVATAR_OBJECT_NAME);
			if (currentAvatar != null)
				Destroy(currentAvatar);

			var avatarObject = new GameObject(AVATAR_OBJECT_NAME);

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
					SetAvatarScale(avatarCode, avatarObject.transform);

					if (useAnimations)
					{
						if (isWebGLDemo)
							// don't show all animations in webgl demo
							GetComponent<AnimationManager>().CreateAnimator(headObject, animations.GetRange(0, 5));
						else
							GetComponent<AnimationManager>().CreateAnimator(headObject, animations);
					}
					else
					{
						//add an empty blendshape with index -1
						availableBlendshapes.Add(-1, "None");

						var mesh = headObject.GetComponentInChildren<SkinnedMeshRenderer>().sharedMesh;
						for (int i = 0; i < mesh.blendShapeCount; i++)
							availableBlendshapes.Add(i, mesh.GetBlendShapeName(i));
						ChangeCurrentBlendshape(-1);
						blendshapesSelectingView.InitItems(availableBlendshapes.Values.ToList());

						if (availableBlendshapes.Count == 1)
							blendshapesPanel.SetActive(false);
					}
				}
			}

			var haircutsIdsRequest = GetHaircutsIdsAsync(avatarCode);
			yield return haircutsIdsRequest;

			string[] haircuts = haircutsIdsRequest.Result;
			if (haircuts != null && haircuts.Length > 0)
			{
				//Add fake "bald" haircut
				var haircutsList = haircuts.ToList();
				haircutsList.Insert(0, BALD_HAIRCUT_NAME);
				avatarHaircuts = haircutsList.ToArray();
				OnDisplayedHaircutChanged(avatarHaircuts[currentHaircut]);
				haircutsSelectingView.InitItems(avatarCode, avatarHaircuts.ToList(), avatarProvider);
				haircutsPanel.SetActive(true);
			}
			else
			{
				haircutsPanel.SetActive(false);
				OnDisplayedHaircutChanged(BALD_HAIRCUT_NAME);
			}

			ChangeControlsInteractability(true);
			avatarControls.SetActive(true);
		}

		/// <summary>
		/// Requests haircuts identities from the server or takes them from the cache
		/// </summary>
		private AsyncRequest<string[]> GetHaircutsIdsAsync(string avatarCode)
		{
			var request = new AsyncRequest<string[]>();
			StartCoroutine(GetHaircutsIdsFunc(avatarCode, request));
			return request;
		}

		private IEnumerator GetHaircutsIdsFunc(string avatarCode, AsyncRequest<string[]> request)
		{
			string[] haircuts = null;
			if (cachedHaircuts.ContainsKey(avatarCode))
				haircuts = cachedHaircuts[avatarCode];
			else
			{
				var haircutsRequest = avatarProvider.GetHaircutsIdAsync(avatarCode);
				yield return request.AwaitSubrequest(haircutsRequest, 1.0f);
				if (request.IsError)
					yield break;

				haircuts = ReorderHaircutIds(haircutsRequest.Result);
				cachedHaircuts[avatarCode] = haircuts;
			}
			request.IsDone = true;
			request.Result = haircuts;
		}

		private string[] ReorderHaircutIds(string[] haircuts)
		{
			if (haircuts == null)
				return null;

			List<string> baseHaircuts = new List<string>();
			List<string> facegenHaircuts = new List<string>();
			foreach(string h in haircuts)
			{
				if (h.Contains("facegen"))
					facegenHaircuts.Add(h);
				else
					baseHaircuts.Add(h);
			}
			baseHaircuts.AddRange(facegenHaircuts);
			return baseHaircuts.ToArray();
		}

		private IEnumerator WaitFaceAvatarCalculations(AsyncRequest<AvatarData> avatarRequest)
		{
			pipelinesPanel.SetActive(true);
			yield return avatarRequest;
			if (!avatarRequest.IsError)
			{
				faceAvatarCode = avatarRequest.Result.code;

				// preload haircuts previews beforehand
				var haircutsIdsRequest = GetHaircutsIdsAsync(faceAvatarCode);
				yield return haircutsIdsRequest;
				if (haircutsIdsRequest.Result != null)
					haircutsSelectingView.InitItems(faceAvatarCode, haircutsIdsRequest.Result.ToList(), avatarProvider);

				faceToggle.interactable = true;
			}
		}

		private void SetAvatarScale(string avatarCode, Transform avatarTransform)
		{
			Vector3 scale = faceAvatarScale;
			string pipelineInfoFile = AvatarSdkMgr.Storage().GetAvatarFilename(avatarCode, AvatarFile.PIPELINE_INFO);
			if (File.Exists(pipelineInfoFile))
			{
				string fileContent = File.ReadAllText(pipelineInfoFile);

				if (fileContent == PipelineType.HEAD.GetPipelineTypeName())
					scale = headAvatarScale;
			}
			avatarTransform.localScale = scale;
		}

		#endregion

		#region Haircut handling

		/// <summary>
		/// Change the displayed haircut. Make controls inactive while haircut is being loaded to prevent
		/// multiple coroutines running at once.
		/// </summary>
		/// <param name="newIdx">New haircut index.</param>
		private IEnumerator ChangeHaircut(int newIdx)
		{
			ChangeControlsInteractability(false);

			var previousIdx = currentHaircut;
			yield return StartCoroutine(ChangeHaircutFunc(newIdx));
			if (previousIdx != currentHaircut)
				OnDisplayedHaircutChanged(avatarHaircuts[currentHaircut]);

			ChangeControlsInteractability(true);
		}

		/// <summary>
		/// Actually load the haircut model and texture and display it in the scene (aligned with the head).
		/// </summary>
		/// <param name="newIdx">Index of the haircut.</param>
		private IEnumerator ChangeHaircutFunc(int newIdx)
		{
			if (newIdx < 0 || newIdx >= avatarHaircuts.Length)
				yield break;

			currentHaircut = newIdx;
			string haircutName = avatarHaircuts[currentHaircut];
			var haircutObj = GameObject.Find(HAIRCUT_OBJECT_NAME);

			// bald head is just absence of haircut
			if (string.Compare(haircutName, BALD_HAIRCUT_NAME) == 0)
			{
				Destroy(haircutObj);
				yield break;
			}

			var haircurtMeshRequest = avatarProvider.GetHaircutMeshAsync(currentAvatarCode, haircutName);
			yield return Await(haircurtMeshRequest);
			if (haircurtMeshRequest.IsError)
				yield break;

			Destroy(haircutObj);

			var texturedMesh = haircurtMeshRequest.Result;
			var meshObject = new GameObject(HAIRCUT_OBJECT_NAME);
			meshObject.AddComponent<MeshFilter>().mesh = texturedMesh.mesh;
			var meshRenderer = meshObject.AddComponent<MeshRenderer>();

			meshRenderer.material = IsUnlitMode ? haircutUnlitMaterial : haircutLitMaterial;
			meshRenderer.material.mainTexture = texturedMesh.texture;

			// Some haircuts looks better with enabled culling
			if (haircutName.Contains("_NewSea_"))
				meshRenderer.material.SetInt("_Cull", (int)UnityEngine.Rendering.CullMode.Back);

			// ensure that haircut is rotated just like the head
			var avatarObject = GameObject.Find(AVATAR_OBJECT_NAME);
			if (avatarObject != null)
			{
				meshObject.transform.SetParent(avatarObject.transform);
				meshObject.transform.localRotation = Quaternion.identity;
				meshObject.transform.localScale = Vector3.one;
			}

			yield return null;  // only after the next frame the textures and materials are actually updated in the scene
		}

		private string HaircutIdToFileName(string haircutId, string fileExtension)
		{
			haircutId = haircutId.Replace('/', '_').Replace('\\', '_');
			return string.Format("haircut_{0}.{1}", haircutId, fileExtension);
		}

		#endregion

		#region Blendshapes handling
		private void ChangeCurrentBlendshape(int newIdx)
		{
			if (!availableBlendshapes.ContainsKey(newIdx))
				return;

			currentBlendshape = newIdx;

			var meshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer>();
			foreach (int idx in availableBlendshapes.Keys)
			{
				if (idx >= 0)
					meshRenderer.SetBlendShapeWeight(idx, idx == currentBlendshape ? 100.0f : 0.0f);
			}

			blendshapeText.text = availableBlendshapes[currentBlendshape];
		}
		#endregion

		#region UI controls handling
		private void ChangeControlsInteractability(bool isEnabled)
		{
			foreach (var control in avatarControls.GetComponentsInChildren<Selectable>())
				control.interactable = isEnabled;

			if (isEnabled && faceAvatarCode == null)
				faceToggle.interactable = false;
		}
		#endregion
	}
}
