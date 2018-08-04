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
using System.Collections.Generic;

namespace ItSeez3D.AvatarSdk.Cloud
{
	[Serializable]
	public class AvatarData
	{
		// avatar id
		public string code = string.Empty;

		// avatar models computing status. One of [Uploading, Queued, Computing, Completed, Failed, Timed Out]
		public string status = string.Empty;

		// current progress of Avatar status. In range [0:100]
		public int progress = 0;

		// avatar name
		public string name = string.Empty;

		// avatar description
		public string description = string.Empty;

		// ISO 8601 datetime
		public string created_on = string.Empty;

		// algorithmic pipeline used to generate this item
		public string pipeline = string.Empty;

		// absolute URL to this avatar
		public string url = string.Empty;

		// absolute URL to retrieve the thumbnail image
		public string thumbnail = string.Empty;

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

		public override bool Equals (object obj)
		{
			var data = obj as AvatarData;
			return data != null &&
				   code == data.code &&
				   status == data.status &&
				   progress == data.progress &&
				   name == data.name &&
				   description == data.description &&
				   created_on == data.created_on &&
				   pipeline == data.pipeline &&
				   url == data.url &&
				   thumbnail == data.thumbnail &&
				   mesh == data.mesh &&
				   texture == data.texture &&
				   preview == data.preview &&
				   haircuts == data.haircuts &&
				   blendshapes == data.blendshapes;
		}

		public override int GetHashCode ()
		{
			var hashCode = 1081997394;
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (code);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (status);
			hashCode = hashCode * -1521134295 + progress.GetHashCode ();
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (name);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (description);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (created_on);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (pipeline);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (url);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (thumbnail);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (mesh);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (texture);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (preview);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (haircuts);
			hashCode = hashCode * -1521134295 + EqualityComparer<string>.Default.GetHashCode (blendshapes);
			return hashCode;
		}
	}
}