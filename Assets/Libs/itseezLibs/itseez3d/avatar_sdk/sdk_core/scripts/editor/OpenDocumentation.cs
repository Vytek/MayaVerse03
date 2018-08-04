/* Copyright (C) Itseez3D, Inc. - All Rights Reserved
* You may not use this file except in compliance with an authorized license
* Unauthorized copying of this file, via any medium is strictly prohibited
* Proprietary and confidential
* UNLESS REQUIRED BY APPLICABLE LAW OR AGREED BY ITSEEZ3D, INC. IN WRITING, SOFTWARE DISTRIBUTED UNDER THE LICENSE IS DISTRIBUTED ON AN "AS IS" BASIS, WITHOUT WARRANTIES OR
* CONDITIONS OF ANY KIND, EITHER EXPRESS OR IMPLIED
* See the License for the specific language governing permissions and limitations under the License.
* Written by Itseez3D, Inc. <support@itseez3D.com>, April 2017
*/

#if UNITY_EDITOR && !UNITY_WEBGL

using UnityEditor;

namespace ItSeez3D.AvatarSdk.Core.Editor
{
	/// <summary>
	/// Open documentation main page through menu.
	/// </summary>
	[InitializeOnLoad]
	public static class OpenDocumentation
	{
		[MenuItem ("Window/itSeez3D Avatar SDK/Documentation")]
		public static void OpenDocumentationHtml ()
		{
			DocumentationHelper.OpenDocumentationInBrowser("index.html");
		}
	}
}
#endif
