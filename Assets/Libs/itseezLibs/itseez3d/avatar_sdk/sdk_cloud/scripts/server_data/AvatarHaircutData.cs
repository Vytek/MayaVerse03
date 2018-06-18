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
	public class AvatarHaircutData
	{
		// unique identifier of a haircut
		public string identity = string.Empty;

		// haircut formal gender. One of [male, female, unisex].
		public string gender = string.Empty;

		// absolute URL to preview of Avatar model with this haircut applied
		public string preview = string.Empty;

		// absolute URL to this haircut
		public string url = string.Empty;

		// absolute URL to haircut png texture
		public string texture = string.Empty;

		// absolute URL to Avatar model
		public string model = string.Empty;

		// absolute URL to per-avatar haircut point cloud zip
		public string pointcloud = string.Empty;

		// absolute URL to per-avatar haircut mesh zip
		public string mesh = string.Empty;
	}
}

