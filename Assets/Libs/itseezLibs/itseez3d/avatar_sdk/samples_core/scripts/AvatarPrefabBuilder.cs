/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

#if UNITY_EDITOR
using System.IO;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using ItSeez3D.AvatarSdkSamples.Core;
using UnityEditor;
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public static class AvatarPrefabBuilder
	{
		private class PrefabData
		{
			public List<string> savedAssets = new List<string> ();
			public UnityEngine.Object createdPrefab = null;
		}

		public static void CreateAvatarPrefab (GameObject avatarObject, string headObjectName, string haircutObjectName, string avatarId)
		{
			var prefabDir = Utils.EnsureEditorDirectoryExists ("itseez3d_prefabs", avatarId);
			CreatePrefab (prefabDir, avatarObject, headObjectName, haircutObjectName);
			EditorUtility.DisplayDialog ("Prefab created successfully!", string.Format ("You can find your prefab in '{0}' folder", prefabDir), "Ok");
		}

#if AVATAR_ASSET_BUNDLE
		public static void CreateAvatarAssetBundle(string assetBundlePath, GameObject avatarObject, string headObjectName, string haircutObjectName)
		{
			string prefabDir = "Assets/itseez3d_prefabs";
			Directory.CreateDirectory(prefabDir);

			PrefabData prefabData = CreatePrefab(prefabDir, avatarObject, headObjectName, haircutObjectName);

			UnityEngine.Object[] selection = new UnityEngine.Object[] { prefabData.createdPrefab };
			UnityEditor.BuildPipeline.BuildAssetBundle(prefabData.createdPrefab, selection, assetBundlePath,
				UnityEditor.BuildAssetBundleOptions.CollectDependencies | UnityEditor.BuildAssetBundleOptions.CompleteAssets,
				UnityEditor.BuildTarget.StandaloneWindows);

			foreach (string asset in prefabData.savedAssets)
				UnityEditor.AssetDatabase.DeleteAsset(asset);
		}
#endif

		private static PrefabData CreatePrefab (string prefabDir, GameObject avatarObject, string headObjectName, string haircutObjectName)
		{
			PrefabData prefabData = new PrefabData ();
			avatarObject = GameObject.Instantiate (avatarObject);
			GameObject headObject = GetChildByName (avatarObject, headObjectName);
			GameObject hairObject = GetChildByName (avatarObject, haircutObjectName);

			if (headObject != null) {
				SkinnedMeshRenderer headMeshRenderer = headObject.GetComponentInChildren<SkinnedMeshRenderer> ();
				headMeshRenderer.material.mainTexture = InstantiateAndSaveAsset (headMeshRenderer.material.mainTexture, Path.Combine (prefabDir, "head_texture.mat"), ref prefabData.savedAssets);
				headMeshRenderer.material = InstantiateAndSaveAsset (headMeshRenderer.material, Path.Combine (prefabDir, "head_material.mat"), ref prefabData.savedAssets);
				headMeshRenderer.sharedMesh = InstantiateAndSaveAsset (headMeshRenderer.sharedMesh, Path.Combine (prefabDir, "headMesh.asset"), ref prefabData.savedAssets);
			}

			if (hairObject != null) {
				MeshFilter hairMeshFilter = hairObject.GetComponentInChildren<MeshFilter> ();
				MeshRenderer hairMeshRenderer = hairObject.GetComponentInChildren<MeshRenderer> ();
				hairMeshRenderer.material.mainTexture = InstantiateAndSaveAsset (hairMeshRenderer.material.mainTexture, Path.Combine (prefabDir, "hair_texture.mat"), ref prefabData.savedAssets);
				hairMeshRenderer.material = InstantiateAndSaveAsset (hairMeshRenderer.material, Path.Combine (prefabDir, "hair_material.mat"), ref prefabData.savedAssets);
				hairMeshFilter.mesh = InstantiateAndSaveAsset (hairMeshFilter.mesh, Path.Combine (prefabDir, "hairMesh.asset"), ref prefabData.savedAssets);
			}

			UnityEditor.AssetDatabase.SaveAssets ();

			GameObject.Destroy (avatarObject.GetComponentInChildren<RotateByMouse> ());

			string prefabPath = prefabDir + "/avatar.prefab";
			prefabData.createdPrefab = UnityEditor.PrefabUtility.CreatePrefab (prefabPath, avatarObject);
			prefabData.savedAssets.Add (prefabPath);
			GameObject.Destroy (avatarObject);
			return prefabData;
		}

		private static T InstantiateAndSaveAsset<T> (T obj, string assetPath, ref List<string> savedAssetsList) where T : UnityEngine.Object
		{
			T instance = GameObject.Instantiate (obj);
			UnityEditor.AssetDatabase.CreateAsset (instance, assetPath);
			savedAssetsList.Add (assetPath);
			return instance;
		}

		private static GameObject GetChildByName (GameObject obj, string name)
		{
			var children = obj.GetComponentsInChildren<Transform> ();
			foreach (var child in children) {
				if (child.name.ToLower () == name.ToLower ())
					return child.gameObject;
			}

			return null;
		}
	}
}
#endif
