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
using ItSeez3D.AvatarSdk.Core;

namespace ItSeez3D.AvatarSdkSamples.Core
{
	public delegate IEnumerator AsyncRequestAwaiter(AsyncRequest r);

	/// <summary>
	/// Interface defines methods that have different implementation for Cloud and Offline avatar viewer.
	/// </summary>
	public interface IViewerImplementation 
	{
		// Indicates whether the converting to obj is enabled
		bool IsObjConvertEnabled { get; }

		// Converts avtar to obj format
		void ConvertAvatarToObjFormat(string avatarId, string haircutId, Color haircutColor, Vector4 tint);

		// Indicates whether the export to FBX is enabled
		bool IsFBXExportEnabled { get; }

		// Performs export to FBX
		void ExportAvatarAsFBX(string avatarId, string haircutId, Color haircutColor, Vector4 tint);
	}
}
