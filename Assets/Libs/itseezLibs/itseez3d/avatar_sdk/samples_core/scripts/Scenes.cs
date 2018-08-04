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
using UnityEngine;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public enum SceneType
	{
		AVATAR_VIEWER,
	}

	public static class Scenes
	{
		private static Dictionary<SceneType, string> sceneNames = new Dictionary<SceneType, string>()
		{
			{ SceneType.AVATAR_VIEWER, "itseez3d/avatar_sdk/samples_core/scenes/avatar_viewer" },
		};

		public static string GetSceneName(SceneType scene)
		{
			return sceneNames[scene];
		}
	}
}
