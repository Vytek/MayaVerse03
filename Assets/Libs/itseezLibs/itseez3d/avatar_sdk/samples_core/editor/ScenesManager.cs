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
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace ItSeez3D.AvatarSdkSamples.Core.Editor
{
	[InitializeOnLoad]
	public class ScenesManager
	{
		private static List<string> scenesWithViewer = new List<string> () {
			"Assets/itseez3d/avatar_sdk/samples_cloud/02_gallery_sample_cloud/scenes/02_gallery_sample_cloud.unity",
			"Assets/itseez3d/avatar_sdk/samples_cloud/06_webgl_sample/scenes/06_webgl_sample.unity",
			"Assets/itseez3d/avatar_sdk/samples_cloud/05_resources_sample_cloud/scenes/05_resources_sample_cloud.unity",
			"Assets/itseez3d/avatar_sdk/samples_offline/02_gallery_sample_offline/scenes/02_gallery_sample_offline.unity",
			"Assets/itseez3d/avatar_sdk/samples_offline/05_resources_sample_offline/scenes/05_resources_sample_offline.unity"
		};

		private static string viewerScenePath = "Assets/itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer.unity";

		static ScenesManager ()
		{
			EditorSceneManager.sceneOpened += (s, m) => {
				EnableOpenedScenesInBuildSettings ();
			};

			EditorSceneManager.sceneClosed += s => {
				EnableOpenedScenesInBuildSettings ();
			};
		}

		private static void EnableOpenedScenesInBuildSettings ()
		{
			List<string> openedScenes = new List<string> ();
			for (int i = 0; i < EditorSceneManager.sceneCount; i++) {
				Scene s = EditorSceneManager.GetSceneAt (i);
				if (s.isLoaded)
					openedScenes.Add (s.path);
			}
			EnableScenesInBuildSettings (openedScenes);
		}

		private static void EnableScenesInBuildSettings (List<string> scenes)
		{
			List<EditorBuildSettingsScene> scenesInBuildSettings = new List<EditorBuildSettingsScene> ();
			foreach (string scene in scenes) {
				AddSceneIfNotExists (scenesInBuildSettings, scene);
				if (scenesWithViewer.Contains (scene))
					AddSceneIfNotExists (scenesInBuildSettings, viewerScenePath);
			}
			if (scenesInBuildSettings.Count != 1 || scenesInBuildSettings[0].path != viewerScenePath)
				EditorBuildSettings.scenes = scenesInBuildSettings.ToArray ();
		}

		private static void AddSceneIfNotExists (List<EditorBuildSettingsScene> scenesList, string scenePath)
		{
			EditorBuildSettingsScene existedScene = scenesList.FirstOrDefault (s => string.Compare (s.path, scenePath) == 0);
			if (existedScene == null)
				scenesList.Add (new EditorBuildSettingsScene (scenePath, true));
		}
	}
}
#endif
