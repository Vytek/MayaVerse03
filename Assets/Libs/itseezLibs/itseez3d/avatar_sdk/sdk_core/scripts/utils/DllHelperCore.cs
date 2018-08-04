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

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Holds name of the native plugin that does the avatar converting to various formats.
	/// </summary>
	public static class DllHelperCore
	{
#if UNITY_STANDALONE_WIN || UNITY_EDITOR_WIN
		public const string dll = "avatars_shared_core";
#elif UNITY_STANDALONE_OSX || UNITY_EDITOR_OSX
		public const string dll =  "avatars_shared_core";
#else
		public const string dll = "IS_NOT_SUPPORTED_ON_THIS_PLATFORM_YET";
#endif
	}
}