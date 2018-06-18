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
using UnityEngine;
using ItSeez3D.AvatarSdkSamples.Core;
using ItSeez3D.AvatarSdk.Cloud;
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdkSamples.Cloud
{
	public class CloudViewerImplementation : IViewerImplementation
	{
		public bool IsObjConvertEnabled { get { return false; } }

		public void ConvertAvatarToObjFormat(string avatarId, string haircutId, Color haircutColor, Vector4 tint)
		{
			//Converting to obj is unabailable in cloud sample. Do nothing.
		}

		public bool IsFBXExportEnabled { get { return false; } }

		public void ExportAvatarAsFBX(string avatarId, string haircutId, Color haircutColor, Vector4 tint)
		{
			//FBX export is unavalaible in cloud sample. Do nothing.
		}
	}
}
