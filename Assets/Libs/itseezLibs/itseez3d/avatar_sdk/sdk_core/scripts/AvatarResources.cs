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

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Represents subset of avatar resources
	/// </summary>
	public enum AvatarResourcesSubset
	{
		/// <summary>
		/// All available resources
		/// </summary>
		ALL,

		/// <summary>
		/// Default subset of resources
		/// </summary>
		DEFAULT
	}

	public class AvatarResources
	{
		private AvatarResources() { }

		public static AvatarResources Empty
		{
			get { return new AvatarResources(); }
		}

		public void Merge(AvatarResources mergeFromResources)
		{
			foreach (string h in mergeFromResources.haircuts)
				if (!haircuts.Contains(h))
					haircuts.Add(h);

			foreach (string b in mergeFromResources.blendshapes)
				if (!blendshapes.Contains(b))
					blendshapes.Add(b);
		}

		public List<string> blendshapes = new List<string>();

		public List<string> haircuts = new List<string>();
	}
}
