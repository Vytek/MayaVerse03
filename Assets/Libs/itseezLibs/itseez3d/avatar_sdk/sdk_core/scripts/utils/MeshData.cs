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
using UnityEngine;

namespace ItSeez3D.AvatarSdk.Core
{
	/// <summary>
	/// Data required for avatar SDK 3D objects (heads, haircuts).
	/// </summary>
	public class MeshData
	{
		public Vector3[] vertices;
		public int[] triangles;
		public Vector2[] uv;

		/// <summary>
		/// For each vertex in final mesh store index of original vertex.
		/// indexMap is longer than the original list of vertices.
		/// </summary>
		public int[] indexMap;
	}
}

