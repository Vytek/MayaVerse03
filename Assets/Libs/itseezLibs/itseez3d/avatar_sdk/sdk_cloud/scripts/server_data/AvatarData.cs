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

namespace ItSeez3D.AvatarSdk.Cloud
{
	[Serializable]
	public class AvatarData
	{
		// absolute URL to this avatar
		public string url = string.Empty;

		// avatar id
		public string code = string.Empty;

		// avatar models computing status. One of [Uploading, Queued, Computing, Completed, Failed, Timed Out]
		public string status = string.Empty;

		// avatar name
		public string name = string.Empty;

		// avatar description
		public string description = string.Empty;

		// ISO 8601 datetime
		public string created_on = string.Empty;

		// absolute URL to retrieve zipped mesh
		public string mesh = string.Empty;

		// absolute URL to retrieve jpeg texture
		public string texture = string.Empty;

		// absolute URL to retrieve png preview
		public string preview = string.Empty;

		// absolute URL to retrieve list of available haircuts for this avatar
		public string haircuts = string.Empty;

		// absolute URL to retrieve zip archive with all available blendshapes
		public string blendshapes = string.Empty;

		// current progress of Avatar status. In range [0:100]
		public int progress = 0;
	}
}