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
using UnityEngine;
using UnityEngine.SceneManagement;

#if UNITY_EDITOR
using UnityEditor;
#endif

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public class SceneDocumentation : MonoBehaviour
	{
		void Start()
		{
			if (!Application.isEditor)
				gameObject.SetActive(false);
		}

		public void OpenDocumentationForScene()
		{
			if (Application.isEditor) {
				var sceneName = SceneManager.GetActiveScene().name;

				if (sceneName.EndsWith("_cloud") || sceneName.EndsWith("_offline")) {
					sceneName = sceneName.Replace("_cloud", "");
					sceneName = sceneName.Replace("_offline", "");
				}

				DocumentationHelper.OpenDocumentationInBrowser(string.Format("scene_{0}.html", sceneName));
			}
		}
	}

#if UNITY_EDITOR
	[CustomEditor(typeof(SceneDocumentation))]
	public class SceneDocumentationEditor : Editor
	{
		public override void OnInspectorGUI()
		{
			DrawDefaultInspector();
			var sceneDocumentation = (SceneDocumentation)target;
			if (GUILayout.Button("Open Documentation For This Scene"))
				sceneDocumentation.OpenDocumentationForScene();
		}
	}
#endif
}
